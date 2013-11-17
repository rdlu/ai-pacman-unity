/**
 * Pacman com Q-Learning, Por Rodrigo Dlugokenski
 */

using UnityEngine;

using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (CharacterController))]

public class QPacman_Controller : PacMan_Controller	{
	public float EXPLORATION_PROBABILITY = 0.05f;
  	public float LEARNING_RATE = 0.3f;
  	public float DISCOUNT_FACTOR = 0.8f;
	
	protected List<float> featuresWeights = new List<float>();
	protected GameObject[] ghosts;
	
	new protected void Start() {
		ghosts = GameObject.FindGameObjectsWithTag("Respawn");
		//Debug.Log("Numero de fantasmas encontrados "+ghosts.Length);
		base.Start();
	}
	
	/// <summary>
    /// Faz o update da classe. A Unity chama update uma vez por frame a
    /// partir da classe (pai) MonoBehaviour
    /// </summary>
    void Update () {
		ghosts = GameObject.FindGameObjectsWithTag("Respawn");
		
		if(lastNode != null) {
			//Debug.Log ("Conexoes: "+lastNode.Connections.Count);
			foreach(PathNode conn in lastNode.Connections) {
				//Debug.Log("S: "+lastNode.Position+"/ D: "+conn.Position);
				buildFeaturesList(conn);
			}				
		}
			
		
		//atualize os raycasts e procure por colisoes com objetos do cenario
        UpdateRaycasts();
        
		//obtem o componente character controler do objeto de jogo vinculado a essa classe.
        CharacterController controller = GetComponent<CharacterController>();
        
        // se a IA esta ligada
        if(IA_ON){
            
			//se por acaso o melhor caminho for nulo, instancie.
            if(bestPath == null)
                    bestPath = new List<PathNode>();
            
			//se o caminho estah vazio ou se jah eh o momento do proximo update e
			//a distancia da posicao atual do pacam a do ultimo no recuperado eh menor que 0.3
            if(bestPath.Count == 0 || (Time.time > nextUpdate && 
				Vector3.Distance(pacMan.transform.position, lastNode.Position) < .3f)){
                
				//se nao eh a primeira execucao e nao eh possivel reiniciar a busca
                if(!firstRun && !aStar.canReset(pacMan.transform)){
					//coloque o pacman na posicao do ultimo no lido
                    pacMan.transform.position = lastNode.Position;
                }
					
				//reinstancie o A* com a posicao atual do pacman
                aStar = new A_Star(pacMan.transform, true);
                
				//escolha um novo proximo estado ao acaso
                aStar.NextState = aStar.chooseRandomState();
                
				//remova o comentario se quiser visualizar a escolha 
				//dinamica de novas origens e destinos
                //Debug.Log("Origem: " + pacMan.transform.position);	
				//if(Time.time > nextUpdate)
				//	Debug.Log("Proximo! " + aStar.NextState.Position);
				
				//encontre o melhor caminho
                bestPath = aStar.findBestPath();
				
				//atualize o momento do proximo update				
				nextUpdate = Time.time + update;            
            }
            
			//se a lista do melhor caminho nao eh nula e contem elementos
			//atribua o primeiro elemento ao ultimo no sendo deslocado
			if(bestPath != null && bestPath.Count > 0)
            	lastNode = bestPath[0];
            
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
            
			//se a distancia da posicao atual do pacam a do 
			//ultimo no recuperado eh menor que 0.3
			if(Vector3.Distance(controller.transform.position, lastNode.Position)<.3f){
				//remova o primeiro no da lista
				bestPath.RemoveAt(0);	
			
			}
        //se a IA esta desligada, o usuario controla o pacman             
        }else{
                
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
	
	
	//Monta uma nova lista com os valores das features
	protected List<float> buildFeaturesList(PathNode node) {
		List<float> featuresList = new List<float>();
		//Distancia dos fantasmas
		float min_ghost_distance = float.MaxValue;
		foreach(GameObject ghost in ghosts) {
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
		foreach(float features in featuresList) {
			featuresSum += features;
		}
		
		foreach(float features in featuresList) {
			normalizedFeaturesList.Add(features/featuresSum);
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
		
		return normalizedFeaturesList;		
	}
	
}

