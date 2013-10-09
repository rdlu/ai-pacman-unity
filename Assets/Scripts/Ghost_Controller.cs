using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Classe que controla os movimentos dos fantasmas.
/// </summary>
public class Ghost_Controller : MonoBehaviour {	
	
	//o objeto de jogo deste fantasma
	GameObject ghost;
	
	//o objeto de jogo do pacman
	GameObject pacMan;
	
	//instancia para a classe A_Star
    A_Star aStar;
    
	//lista com o melhor caminho escolhido pelo A_Star
    List<PathNode> bestPath;
	
	//ultimo no que foi recuperado da lista com o melhor caminho escolhido pelo A_Star
    PathNode lastNode;
	
	//Raycast eh o objeto utilizado na deteccao de colisoes no cenario
	private RaycastHit hit;
	
	//distancia padrao da checagem do raycast para bloqueios nos eixos x e z
	public float rayBlockedDistX = 0.3f;
	public float rayBlockedDistY = 0.3f;
	
	//indice da camada do terreno
	private int groundMask = 8;
	
	//tempo esperado ateh o proximo update. Padrao = 1s
	public float update = 1;
	
	//velocidade na qual o fantasma se move.
	public float speed = 10;
	
	//tolerancia da ganancia. valores altos o deixam mais aleatorio
	//e valores baixos o deixam mais ganancioso
	public float greed = 0.3f;
	
	//momento do proximo update
	float nextUpdate;
	
	//vetor com as direcoes em que o fantasma sera movido
	Vector3 moveDirection;
	
	//indica se eh a primeira execucao do algortimo
	bool firstRun;
	
	//indica o quanto do caminho escolhido deve ser
	//obrigatoriamente completado antes que se possa
	//escolher um novo caminho
	float walkedPath;
	
	//valor atribuido a gravidade
	float gravity = 20.0f;
	
	/// <summary>
    /// Chamada na inicializacao da classe pelo unity atraves da classe MonoBehaviour. 
    /// Inicia a instancia dessa classe.
    /// </summary>
	void Start(){
		
		//ignore a fisica de colisao da camada groundMask. Essa eh a camada
		//onde estao os fantasmas. Fazendo isso os fantasmas nao colidem uns com
		//os outros nem com pacman. Ainda sao detectados quando se bate neles mas
		//podem atravessar uns aos outros
        Physics.IgnoreLayerCollision(groundMask, groundMask, true);
		Physics.IgnoreLayerCollision(groundMask, groundMask, true);
		
		//como ha mais de um fantasma eh interessante que cada script esteja
		//vinculado a seu fantasma para que ele possa ser encontrado
		ghost = this.gameObject;
        
		//instancia a lista de melhor caminho
        bestPath = new List<PathNode>();
        
		//instancia um novo A_Star
        aStar = new A_Star(this.transform, false);
        
		//define a primeira execucao como verdadeira
        firstRun = true;   
		
		//localize e atribua o objeto do pacman
		pacMan = GameObject.Find("PacMan");
		
		//determinar ao comeco se o valor do caminho atualmente 
		//percorrido eh zero ou o valor maximo permite
		//que se escolha ao acaso se vai comecar perseguindo 
		//o pacman ou se movendo ao acaso
		walkedPath = Random.value > .5? 0 : float.MaxValue;

	}
	
	
	/// <summary>
    /// Faz o update da classe. A Unity chama update uma vez por frame a
    /// partir da classe (pai) MonoBehaviour
    /// </summary>
	void Update(){
		
		//recupere o componente CharacterController ao qual esta classe esta
		//vinculada
		CharacterController controller = GetComponent<CharacterController>();
		
		//se por acaso o melhor caminho for nulo, instancie.
        if(bestPath == null)
                bestPath = new List<PathNode>();
        
		//escolha um valor para a ganancia deste update
		float thisGreed = Random.value;
		
		//se o caminho estah vazio ou se jah eh o momento do proximo update e
		//a distancia da posicao atual do pacam a do ultimo no recuperado eh menor que 0.3
        if(bestPath.Count == 0 || (Time.time > nextUpdate && 
			Vector3.Distance(ghost.transform.position, lastNode.Position) < .3f)){
            
			//se nao eh a primeira execucao e nao eh possivel reiniciar a busca
            if(!firstRun && !aStar.canReset(ghost.transform)){
				//coloque o pacman na posicao do ultimo no lido
                ghost.transform.position = lastNode.Position;
            }
			
			//reinstancie o A* com a posicao atual do pacman
            aStar = new A_Star(ghost.transform, false);
            
			//se esta ganancia eh maior que a ganancia tolerada
			if(thisGreed > greed){
				try{
					//se o pacman esta morto, escolha um novo estado ao acaso
					//senao, escolha o no mais proximo a ele
	      	 		aStar.NextState = Global.DEAD_PACMAN? aStar.chooseRandomState() :
						Global.findClosestNode(pacMan.transform.position, Global.nodes);
				}
				//apesar da verificacao, uma unica vez a excecao foi lancada em razao 
				//do pacman morto por precaucao, um try catch foi adicionado
				catch(MissingReferenceException){
					
					//escolha um novo estado ao acaso
					aStar.NextState = aStar.chooseRandomState();
					//Debug.Log("Excecao");
				}
			}
			//senao
			else{
				
				//instancie um no para a posicao atual do fantasma
				PathNode ghostNode = new PathNode();
				ghostNode.Position = ghost.transform.position;
				
				//procure por este no no vetor de nos
				int index = Global.nodes.IndexOf(ghostNode);
				
				//se nao foi encontrado
				if(index < 0){
					//encontre e atribua o mais proximo a posicao atual do fantasma
					ghostNode = Global.findClosestNode(ghostNode.Position, Global.nodes);
					//Debug.Log("menor que zero");
				}
				
				//faca
				do{
					aStar.NextState = aStar.chooseRandomState();
				}
				//enquanto a distancia entre o no do fantasma e o escolhido for maior que 20
				//(valor escolhido com base em testes para evitar que o fantasma va para 
				//muito longe)
				while(Vector3.Distance(ghostNode.Position, aStar.NextState.Position) < 20);
			}
            
			//remova o comentario se quiser visualizar a escolha 
			//dinamica de novas origens e destinos
            //Debug.Log("Origem: " + pacMan.transform.position);	
			//if(Time.time > nextUpdate)
			//	Debug.Log("Proximo! " + aStar.NextState.Position);
			
			//encontre o melhor caminho
            bestPath = aStar.findBestPath();
			
			//indica que ao menos 5 nos do caminho escolhido devem ser
			//obrigatoriamente completados antes que se possa
			//escolher um novo caminho
			walkedPath = Mathf.Max(0, bestPath.Count - 5);
		
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
		
		//se a ganancia atual eh maior que a tolerada e o numero minimo
		//de nos jah foi lido
		if(thisGreed > greed && bestPath.Count < walkedPath)
			bestPath.Clear();
	}
	
	/// <summary>
	/// Escolhe uma direcao para o movimento do fantasma.
	/// </summary>
	void ChooseDirection(){

		//faca
		do{
			//Random.onUnitSphere retorna um ponto do plano com um raio 1
	        moveDirection = Random.onUnitSphere;
			
	        //o plano esta deitado sobre o eixo y, mantenha-o em zero
	        moveDirection.y = 0;
		}
		//enquanto o fantasma estiver parado
		while (moveDirection.x == 0 && moveDirection.z == 0);
			
		//se o fantasma for se mover na diagonal
		if(moveDirection.x != 0 && moveDirection.z != 0){
		
			//garanta o movinento em uma unica direcao 
			//zerando um dos outros valores
			//escolha um valor aleatorio entre 1 e 0.
			//se o valor eh maior que 0.5
			if(Random.value > .5f){
				
				//zere o eixo x
				moveDirection.x = 0;
			}
			//senao
			else{
				//zere o eixo z
				moveDirection.z = 0;
			}

		}
		//normaliza o vetor deixando seu comprimento 
		//em 1 no maximo
        moveDirection.Normalize();
		
		//multiplica a direcao pela velocidade
        moveDirection *= speed;
		
	}
}
