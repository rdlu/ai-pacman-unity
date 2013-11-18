/**
 * Pacman com Q-Learning, Por Rodrigo Dlugokenski
 */

using UnityEngine;

using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]

public class QPacman_Controller : PacMan_Controller {
  public float EXPLORATION_PROBABILITY = 0.05f;
  public float LEARNING_RATE = 0.3f;
  public float DISCOUNT_FACTOR = 0.8f;

  protected static int NUM_FEATURES = 2;

  protected float[] featuresWeights = new float[NUM_FEATURES];
  protected float[] lastActionFeatures = new float[NUM_FEATURES];
  protected GameObject[] ghosts;
  protected PathNode bestNode = null;

  new protected void Start() {
    ghosts = GameObject.FindGameObjectsWithTag("Respawn");
    //Debug.Log("Numero de fantasmas encontrados "+ghosts.Length);
    base.Start();
  }

  /// <summary>
  /// Faz o update da classe. A Unity chama update uma vez por frame a
  /// partir da classe (pai) MonoBehaviour
  /// </summary>
  void Update() {
    ghosts = GameObject.FindGameObjectsWithTag("Respawn");

    //atualize os raycasts e procure por colisoes com objetos do cenario
    UpdateRaycasts();

    //obtem o componente character controler do objeto de jogo vinculado a essa classe.
    CharacterController controller = GetComponent<CharacterController>();

    // se a IA esta ligada
    if (IA_ON) {
      if (true || bestNode == null ||
        (Random.value > (1f - EXPLORATION_PROBABILITY))) {
          Debug.Log("Explorando");
          bestNode = explore();
      }
      else {
        Debug.Log("Movendo");
        bestNode = move();
      }

      Debug.Log("Melhor Nodo "+bestNode.Position);

      //se a lista do melhor caminho nao eh nula e contem elementos
      //atribua o primeiro elemento ao ultimo no sendo deslocado
      if (bestNode != null)
        lastNode = bestNode;

      //calcule x e z como a diferenca entre o ponto atual do pacman e
      //onde se quer chegar
      float x = lastNode.Position.x - controller.transform.position.x;
      float z = lastNode.Position.z - controller.transform.position.z;

      //instancie move direction com estes valores mantendo y como 0
      moveDirection = new Vector3(x, 0, z);

      //faca a transformacao das coordenadas para a coordenada do mundo
      //rotacionando as coordendas em funcao da rotacao do objeto (this)
      //mantido por precaucao. Nao aparenta ser necessario para o pacman
      moveDirection = transform.TransformDirection(moveDirection);

      //multiplique pela velocidade de deslocammento
      moveDirection *= speed;

      // Aplique a gravidade em funcao do tempo do ultimo frame
      moveDirection.y -= gravity * Time.deltaTime;

      //mova o personagem na direcao do proximo ponto em funcao do tempo do ultimo frame
      controller.Move(moveDirection * Time.deltaTime);
      //se a IA esta desligada, o usuario controla o pacman             
    }
    else {

      //instancia o vetor de movimento em funcao da entrada de dados dos eixos  
      moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
      //faca a transformacao das coordenadas para a coordenada do mundo
      //rotacionando as coordendas em funcao da rotacao do objeto (this)
      //mantido por precaucao. Nao aparenta ser necessario para o pacman
      moveDirection = transform.TransformDirection(moveDirection);

      //multiplique pela velocidade de deslocammento
      moveDirection *= speed;

      // Aplique a gravidade em funcao do tempo do ultimo frame
      moveDirection.y -= gravity * Time.deltaTime;

      //mova o personagem na direcao do proximo ponto em funcao do tempo do ultimo frame
      controller.Move(moveDirection * Time.deltaTime);
    }

    //primeira execucao = false
    firstRun = false;

    //atualize a pontuacao (metodo da classe pai Eat_Pellet)
    base.UpdateScore();


  }

  protected PathNode explore() {
    //se nao eh a primeira execucao e nao eh possivel reiniciar a busca
    if (!firstRun) {
      //coloque o pacman na posicao do ultimo no lido
      pacMan.transform.position = lastNode.Position;
    }
    //no de retorno
    PathNode nodeD = null;

    //faca
    do {
      //recupere os nos do labirinto
      List<PathNode> nodes = Global.nodes;

      //escolha ao acaso um indice da arvore de nos do labirinto
      //subtraia um do tamanho (posicoes validas vao d 0 a count-1)
      int index = (int)(Random.value * (nodes.Count - 1));

      //atribua o no do indice ao no de retorno
      nodeD = nodes[index];
    }
    //enquanto o no de retorno for uma parede
    while (nodeD.Wall);

    //retorne o no selecionado
    return nodeD;
  }

  protected PathNode move() {
    PathNode choosenNode = null;
    if (lastNode != null) {
      //Debug.Log ("Conexoes: "+lastNode.Connections.Count);
      float previousQValue = float.MinValue;
      foreach (PathNode conn in lastNode.Connections) {
        //Debug.Log("S: "+lastNode.Position+"/ D: "+conn.Position);
        float[] currentFeatures = buildFeaturesList(conn);
        float currentQValue = getQValue(currentFeatures);
        choosenNode = (currentQValue > previousQValue) ? conn : choosenNode;
        lastActionFeatures = (currentQValue > previousQValue) ? currentFeatures : lastActionFeatures;
      }
    }

    List<PathNode> ret = new List<PathNode>();
    ret.Add(choosenNode);
    return choosenNode;
  }

  protected float getQValue(float[] featuresValues) {
    return featuresValues.Length == 0 ? float.MinValue : DotProduct(featuresWeights,featuresValues);
  }

  //Monta uma nova lista com os valores das features
  protected float[] buildFeaturesList(PathNode node) {
    List<float> featuresList = new List<float>();
    //Distancia dos fantasmas
    float min_ghost_distance = float.MaxValue;
    foreach (GameObject ghost in ghosts) {
      //current_a_star.findBestPath(this.lastNode, ghost.lastNode);
      //distancia "
      float distance = Vector3.Distance(node.Position, ghost.transform.position);
      min_ghost_distance = (distance < min_ghost_distance) ? distance : min_ghost_distance;
    }
    //Debug.Log("Distancia do Fantasma Mais Proximo: "+min_ghost_distance);
    featuresList.Add(min_ghost_distance);

    //Distancia da comida
    float food_distance = Vector3.Distance(node.Position, node.bfs_pellet_position());
    //Debug.Log("Distancia da COMIDA Mais Proxima: "+food_distance);
    featuresList.Add(food_distance);


    //Normalizaçao retorna a porcentagem de participaçao da feature		
    List<float> normalizedFeaturesList = new List<float>();
    float featuresSum = 0f;
    foreach (float features in featuresList) {
      featuresSum += features;
    }

    foreach (float features in featuresList) {
      normalizedFeaturesList.Add(features / featuresSum);
    }

    /* //Debug das duas listas
    string L1 = "";string L2 = "";
    foreach(float features in featuresList) {
      L1 += features.ToString()+" ";
    }
    foreach(float nFeatures in normalizedFeaturesList) {
      L2 += nFeatures.ToString()+" ";
    }
    Debug.Log("Lista: "+L1+" | Normalizada: "+L2);
    */
    float[] normalizedFeaturesArray = normalizedFeaturesList.ToArray();
    if (normalizedFeaturesArray.Length != NUM_FEATURES)
      Debug.LogError("Número de features calculados diferentes do esperado", this);

    return normalizedFeaturesArray;
  }

  //http://rosettacode.org/wiki/Dot_product#C.23
  private static decimal DotProduct(decimal[] vec1, decimal[] vec2) {
    if (vec1 == null)
      return 0;

    if (vec2 == null)
      return 0;

    if (vec1.Length != vec2.Length)
      return 0;

    decimal tVal = 0;
    for (int x = 0; x < vec1.Length; x++) {
      tVal += vec1[x] * vec2[x];
    }

    return tVal;
  }

  private static float DotProduct(float[] vec1, float[] vec2) {
    if (vec1 == null)
      return 0;

    if (vec2 == null)
      return 0;

    if (vec1.Length != vec2.Length)
      return 0;

    float tVal = 0;
    for (int x = 0; x < vec1.Length; x++) {
      tVal += vec1[x] * vec2[x];
    }

    return tVal;
  }

}

