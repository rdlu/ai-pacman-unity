using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Classe de controle do jogo. Para este projeto,
/// foi vinculada ao objeto Main Camera
/// </summary>
public class Global : MonoBehaviour {
	
	//valor padrao para a distancia entre os elementos do cenario
	public static int offSet = 2;
	
	//indice se o pacman morreu
	public static bool DEAD_PACMAN;
	//indica se o pacam estah invencivel
	public static bool UPGRADE;
	//material usado para a aprencia dos fantasmas com medo
	public static Material FEAR;
	
	//armazena a lsita dos colisores dos objetos de jogo dos fantasmas
	public static List<Collider> ghostsColliders;
	//armazena a lista de fantasmas a serem revividos (respawn)
	public static List<GhostRespawn> ghostsRespawn;

	//lista estatica com os nos do cenario. Pode ser acessada 
	//diretamente por outras classes e eh construida pela classe
	//BuildMaze
	public static List<PathNode> nodes;
	
	//tempo de espera entre as reinicializacoes do jogo apos
	//a morte do pacman
	public float restartDelay = 2.0f;
	
	//duracao do tempo de invencibilidade do pacman
	public float upgradeDuration = 3.0f;
	
	//tempo de espera ateh que um fantasma morte volte ao cenario
	public float respawnTime = 5.0f;
	
	//variavel estatica para acesso ao tempo de respawn do fantasma
	//variaveis estaticas nao podem ser definidas pelo unity inspector
	//por isso a colocacao de duas variaveis para o mesmo valor
	public static float RESPAWN_TIME;
	
	//momento em que acaba o atual upgrade
	float upgradeTime = 0f;
	
	//momento em que o jogo deve ser reiniciado
	float restartTime = 0f;
	
	/// <summary>
    /// Chamada na inicializacao da classe pelo unity atraves da classe MonoBehaviour. 
    /// Inicia a instancia dessa classe.
    /// </summary>
	void Start () {
	
		//atribui o tempo de respawn definido pelo usuario a variavel de acesso global
		RESPAWN_TIME = respawnTime;
		
		//localize o material usado para os fantasmas com medo
		//soh eh possivel localizar com a funcao Load objetos 
		//que estejam dentro da pasta Resource
		FEAR = Resources.Load("Materials/Fear") as Material;
		
		//instancia a lista de colisores dos fantasmas
		ghostsColliders = new List<Collider>();
		//inicia a lista de fantasmas que devem ser revividos
		ghostsRespawn = new List<GhostRespawn>();
		
		//para cada objeto de jogo com a tag respawn
		foreach(GameObject obj in GameObject.FindGameObjectsWithTag("Respawn")){
			//adicione a lista de colisores
			ghostsColliders.Add(obj.collider);
		}
	}
	
	/// <summary>
    /// Faz o update da classe. A Unity chama update uma vez por frame a
    /// partir da classe (pai) MonoBehaviour
    /// </summary>
	void Update () {
		
		//se ha fantasmas aguardando por respawn e o primeiro nao
		//eh nulo
		if(ghostsRespawn.Count > 0 && ghostsRespawn[0] != null){
			
			//se o momento atual eh maior que o timer do fantasma
			if(Time.time > ghostsRespawn[0].timer){
				
				//tenta capturar possiveis excecoes
				try{
					
					//Obs: Cada objeto instanciado a partir de um objeto pre-fabricado (PREFAB) possui a palavra
					//(CLONE) adicionada ao seu nome. Para localizar o objeto a partir do nome do ultimo destruido 
					//pode ser necessario remover este trecho. Logo:
					//se a string nao possui '(' atribua a string a variavel ghost. 
					//Senao, divida em '(' e atribua o primeiro trecho
					string ghost =  !ghostsRespawn[0].ghost.Contains("(")? ghostsRespawn[0].ghost:
						ghostsRespawn[0].ghost.Split(new char[]{'('})[0];
					
					//localiza e instancia o objeto (um dos quatro fantasmas) como um GameObject
					//apenas objetos dentro da pasta Resources podem ser encontrados 
					//pelo metodo Load
					GameObject objClone = 
						Instantiate(Resources.Load("Prefabs/" + ghost, typeof(GameObject))) as GameObject;
					
					//adicione o colisor desse fantasma a lista de colisores
					ghostsColliders.Add(objClone.collider);
					
					//remova o nome desse fantasma da lista de respawn
					ghostsRespawn.RemoveAt(0);
					
				}
				//se houve uma excecao
				catch(System.Exception ex){
					//exiba a excecao a fim de identificar e corrigir o problema
					Debug.Log(ex.StackTrace);
				}
			}
		}
		
		//se o pac_man estah morto
		if(DEAD_PACMAN){
			
			//se o momento do restart atualmente eh zero
			if(restartTime == 0f){
				
				//defina o momento do restart como o tempo de jogo atual mais o delay
				restartTime = Time.time + restartDelay;
			}
			
			//chame o metodo de restart
			Restart();
		}
		
		//se o pacman estah invencicel
		if(UPGRADE){
			
			//se o tempo do fim do upgrade eh atualmente zero
			if (upgradeTime == 0f){
				
				//defina o momento limite do upgrade como o tempo de jogo 
				//atual mais o a duracao do upgrade
				upgradeTime = Time.time + upgradeDuration;
				
				//para cada colisor na lista de colisores dos fantasmas
				foreach(Collider ghostC in ghostsColliders){
					//atribua o material para medo
					SetGhostMaterial(ghostC, true);
				}
			}
			
			//se o tempo de upgrade acabou
			if(Time.time > upgradeTime){
				
				//indique a variavel upgrade como falsa
				UPGRADE = false;
				
				//zere o tempo de upgrade
				upgradeTime = 0f;
				
				//para cada colisor na lista de colisores dos fantasmas
				foreach(Collider ghostC in ghostsColliders){
					
					//atribua o material original do fantasma
					SetGhostMaterial(ghostC, false);
				}
			}
		}
	}
	
	/// <summary>
	/// Define o material do fantasma como seu material 
	/// original ou o da aparencia de medo(ativa durante 
	/// upgrade do pacman)
	/// </summary>
	/// <param name='obj'>
	/// O objeto collider do fantasma.
	/// </param>
	/// <param name='fear'>
	/// Flag do material a ser escolhido: 
	/// true - medo; false - material original
	/// </param>
	void SetGhostMaterial(Collider obj, bool fear){
		
		//tenta capturar possiveis excecoes
		try{
			
			//objeto do jogo do colisor (o fantasma)
			GameObject ghost = obj.gameObject;
			
			//se fear eh falso
			if(!fear){
				
				//verifique qual eh o fantasma e recupere e atribua seu material original
				if(ghost.name.Contains("Clyde")){
					ghost.renderer.material = Resources.Load("Materials/Clyde") as Material;
				} else if (ghost.name.Contains("Blinky")){
					ghost.renderer.material = Resources.Load("Materials/Blinky") as Material;
				} else if (ghost.name.Contains("Inky")){
					ghost.renderer.material = Resources.Load("Materials/Inky") as Material;
				} else if (ghost.name.Contains("Pinky")){
					ghost.renderer.material = Resources.Load("Materials/Pinky") as Material;
				}
			}
			//senao
			else
				//localize e atribua o material de medo para o fantasma
				obj.gameObject.renderer.material = FEAR;
			
		}
		//captura a excecao de nao conseguir localizar o material e exibe no console
		catch(MissingReferenceException ex){
			Debug.Log("Nao foi possivel localizar o material");
			Debug.Log(ex.StackTrace);
		}
	}
	
	/// <summary>
	/// Reinicia essa instancia da classe.
	/// </summary>
	void Restart(){
		
		//se o tempo de jogo atual eh maior que 
		//o momento de reiniciar o jogo
		if(Time.time > restartTime){
			
			//recarregue a cena
			Application.LoadLevel(0);
			
			//zero os tempos de upgrade e de restart
			upgradeTime = 0f;
			restartTime = 0f;
			
			//defina o pacman como vivo e vulneravel
			DEAD_PACMAN = false;
			UPGRADE = false;
		}
	}
	
	/// <summary>
	/// encontra e atribui as conexoes de um no do labirinto. Esta fixado 
	/// para procurar por paredes a cada duas unidades nos dois sentidos.
	/// Pode ser reescrito para procurar por nos em distancias desiguais.
	/// </summary>
	/// <param name='node'>
	/// O no para o qual se esta buscando conexoes.
	/// </param>
	/// <param name='forceNode'>
	/// indica se, caso uma conexao nao tenha sido encontrada, um novo 
	/// no deve ser criado para ela.
	/// </param>
	public static void buildConnections(PathNode node, List<PathNode> nodes, bool forceNode){
		
		//posicao do no
		Vector3 pos = node.Position;
		
		//no auxiliar
		PathNode auxNode = new PathNode();
		
		//posicao para o no auxiliar, indica uma conexao
		auxNode.Position = new Vector3(pos.x - offSet, pos.y, pos.z);
		
		//custo para o no auxiliar
		float maxCost = float.MaxValue;
		
		//indice do no de conexao buscado
		int index = nodes.IndexOf(auxNode);
		
		
		//se encontrou o no
		if(index >= 0)
			//adicione o no de conexao a lista de conexoes do no atual
			node.AddConnnection(nodes[index]);
		//se nao encontrou e deve forcar
		else if (forceNode){
			//instancie o novo no de conexao
			PathNode nodeF = new PathNode();
			//defina o custo maximo
			nodeF.Cost = maxCost;
			//como o no de fato nao existe, eh inalcancavel no labirinto
			//marque-o como uma parede
			nodeF.Wall = true;
			//atribua a posicao para este no
			nodeF.Position = new Vector3(pos.x - offSet, pos.y, pos.z);
			
			//adicione-o a lista de conexoes
			node.AddConnnection(nodeF);
		}
		
		//atualize a posicao do no auxiliar
		auxNode.Position = new Vector3(pos.x + offSet, pos.y, pos.z);
		
		//busque um no para essa nova posicao
		index = nodes.IndexOf(auxNode);
		
		//os proximos if else sao analogos ao primeiro
		//se encontrou o no
		if(index >= 0)
			node.AddConnnection(nodes[index]);
		//se nao encontrou e deve forcar
		else if (forceNode){
			PathNode nodeF = new PathNode();
			nodeF.Cost = maxCost;
			nodeF.Wall = true;
			nodeF.Position = new Vector3(pos.x + offSet, pos.y, pos.z);
			
			node.AddConnnection(nodeF);
		}
		
		//atualize a posicao do no auxiliar
		auxNode.Position = new Vector3(pos.x, pos.y, pos.z  - offSet);
		
		//busque um no para essa nova posicao
		index = nodes.IndexOf(auxNode);
		
		//se encontrou o no
		if(index >= 0)
			node.AddConnnection(nodes[index]);
		//se nao encontrou e deve forcar
		else if (forceNode){
			PathNode nodeF = new PathNode();
			nodeF.Cost = maxCost;
			nodeF.Wall = true;
			nodeF.Position = new Vector3(pos.x, pos.y, pos.z  - offSet);
			
			node.AddConnnection(nodeF);
		}
		
		//atualize a posicao do no auxiliar
		auxNode.Position = new Vector3(pos.x, pos.y, pos.z  + offSet);
		
		//busque um no para essa nova posicao
		index = nodes.IndexOf(auxNode);
		
		//se encontrou o no
		if(index >= 0)
			node.AddConnnection(nodes[index]);
		//se nao encontrou e deve forcar
		else if (forceNode){
			PathNode nodeF = new PathNode();
			nodeF.Cost = maxCost;
			nodeF.Wall = true;
			nodeF.Position = new Vector3(pos.x, pos.y, pos.z  + offSet);
			
			node.AddConnnection(nodeF);
		}
	}	
	
	/// <summary>
	/// Encontra o no que esta mais proximo da posicao atual.
	/// </summary>
	/// <returns>
	/// O no mais proximo.
	/// </returns>
	/// <param name='pos'>
	/// Posicao para a qual se estah buscando um no proximo
	/// </param>
	/// <param name='nodes'>
	/// Lista de nos a serem buscados.
	/// </param>
	public static PathNode findClosestNode(Vector3 pos, List<PathNode> nodes){
		
		//crie um no de retorno
		PathNode node = null;
		
		node = new PathNode();
		
		node.Position = pos;
		
		int index = nodes.IndexOf(node);
		
		if(index >=0)
			return nodes[index];
		
		//guarda o custo do proximo destino
		float dist = float.MaxValue;
		
		//para cada no da lista
		foreach(PathNode aNode in nodes){
		
			//calcule a distancia ateh esse no
			float thisDist = Vector3.Distance(pos, aNode.Position);
			
			//se a distancia ateh esse no eh menor que a menor distancia
			if(thisDist < dist){
				
				//atribua essa distancia a menor distancia
				dist = thisDist;
				//atribua esse no ao no de retorno
				node = aNode;
			}
		}
		
		//retorne o no
		return node;
		
	}
}
