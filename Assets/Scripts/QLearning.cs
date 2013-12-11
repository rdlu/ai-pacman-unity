using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (CharacterController))]

public class QLearning : MonoBehaviour {
	//instancia para a classe A_Star
	protected A_Star aStar;

	public float EXPLORATION_PROBABILITY = 0.05f;
	public float LEARNING_RATE = 0.3f;
	public float DISCOUNT_FACTOR = 0.8f;

	protected static int NUM_FEATURES = 2;
	
	protected float[] featuresWeights = new float[NUM_FEATURES];
	protected float[] lastActionFeatures = new float[NUM_FEATURES];
	protected GameObject[] ghosts;
	protected PathNode bestNode = null;
	
	// Use this for initialization
	void Start () {
		ghosts = GameObject.FindGameObjectsWithTag("Respawn");

	}
	
	// Update is called once per frame
	void Update () {
		List<PathNode> nos = Global.nodes;

		PathNode pellet = null;

		Vector3 pacmanPos = this.gameObject.transform.position;

		PathNode currentNode = Global.findClosestNode(pacmanPos, nos);
		//float[] features = buildFeaturesList(currentNode); 
		float previousQValue = float.MinValue;
		PathNode choosenNode = null;
		foreach (PathNode conn in currentNode.Connections) {
			//Debug.Log("S: "+currentNode.Position+"/ D: "+conn.Position);
			float[] currentFeatures = buildFeaturesList(conn);
			float currentQValue = getQValue(currentFeatures);
			//Debug.Log("S: "+currentNode.Position+"/ D: "+conn.Position+"/ Q-Value: "+currentQValue);
			choosenNode = (currentQValue > previousQValue) ? conn : choosenNode;
			lastActionFeatures = (currentQValue > previousQValue) ? currentFeatures : lastActionFeatures;
		}
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

     //Debug das duas listas
    string L1 = "";string L2 = "";
    foreach(float features in featuresList) {
      L1 += features.ToString()+" ";
    }
    foreach(float nFeatures in normalizedFeaturesList) {
      L2 += nFeatures.ToString()+" ";
    }
    Debug.Log("Lista: "+L1+" | Normalizada: "+L2);
    
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
