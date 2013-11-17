using UnityEngine;
//using UnityEditor;

using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (CharacterController))]

/// <summary>
/// Classe com os metodos de controle do pacman. Herda Eat_Pellet para simplificar 
/// a contabilizacao dos fanstasmas comidos na pontuacao do jogo
/// </summary>
public class PacMan_Controller : Eat_Pellet {
    
	//indica a primeira execucao. Usada no controle das atribuicoes do algoritmo
    protected bool firstRun;
    
	//vetor com as direcoes em que o pacman sera movido
    protected Vector3 moveDirection;
    
	//o objeto de jogo do pacman
    protected GameObject pacMan;
    
	//instancia para a classe A_Star
    protected A_Star aStar;
    
	//lista com o melhor caminho escolhido pelo A_Star
    protected List<PathNode> bestPath;
	
	//ultimo no que foi recuperado da lista com o melhor caminho escolhido pelo A_Star
    protected PathNode lastNode;
    
    //indice da camada do terreno
    protected int groundMask = 8;
	
	//momento do proximo update
	protected float nextUpdate;
	
	//Raycast eh o objeto utilizado na deteccao de colisoes no cenario
    protected RaycastHit hit;
    
	//distancia padrao da checagem do raycast para bloqueios nos eixos x e z
    public float rayBlockedDistX = 0.3f;
    public float rayBlockedDistZ = 0.3f;
	
	//velocidade na qual o pacman se move. Padrao = 6
    public float speed = 6.0f;
	
	//tempo esperado ateh o proximo update
	public float update = 3.0f;
	
	//valor atribuido a gravidade
    public float gravity = 20.0f;
    
	//indica se a IA esta ligada. Caso desligada o usuario
	//controla o pacam
    public bool IA_ON = true;
	
    /// <summary>
    /// Chamada na inicializacao da classe pelo unity atraves da classe MonoBehaviour 
    /// (herdade aqui de Eat_Pellet). Inicia a instancia dessa classe.
    /// </summary>
    protected void Start () {
            
        pacMan = this.gameObject; //caso o script nao esteja vinculado ao pacman
		//eh possivel fazer GameObject.Find("PacMan");
		
        //ignore a fisica de colisao da camada groundMask. Essa eh a camada
		//onde estao os fantasmas. Fazendo isso o pacman ainda os detecta qndo
		//bate neles mas simplesmente os atravessa
        Physics.IgnoreLayerCollision(groundMask, groundMask, true);
        
		//instancia a lista de melhor caminho
        bestPath = new List<PathNode>();
        
		//instancia um novo A_Star
        aStar = new A_Star(this.transform, true);
        
		//define a primeira execucao como verdadeira
        firstRun = true;   
    }
        
    /// <summary>
    /// Faz o update da classe. A Unity chama update uma vez por frame a
    /// partir da classe (pai) MonoBehaviour
    /// </summary>
    void Update () {
		
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
    
	/// <summary>
	/// Atualiza a deteccao das colisoes com objetos do cenario.
	/// </summary>
    protected void UpdateRaycasts(){
        
		//verifique se o personagem estah no bloqueado acima lancando dois raios de cada lado dele
        if (Physics.Raycast(new Vector3(transform.position.x-0.3f,transform.position.y,transform.position.z+0.3f), Vector3.up, out hit, rayBlockedDistZ) || 
                Physics.Raycast(new Vector3(transform.position.x+0.3f,transform.position.y,transform.position.z+0.3f), Vector3.up, out hit,rayBlockedDistZ))
        {       
            //se colidiu com um fantasma
            if(Global.ghostsColliders.Contains(hit.collider)){
				//chame o metodo kill
                Kill(hit.collider);
            }
                
        }
        
        //verifique se o personagem estah no bloqueado abaixo lancando dois raios de cada lado dele
        if (Physics.Raycast(new Vector3(transform.position.x-0.3f,transform.position.y,transform.position.z+0.3f), -Vector3.up, out hit,rayBlockedDistZ) || 
                Physics.Raycast(new Vector3(transform.position.x+0.3f,transform.position.y,transform.position.z+0.3f), -Vector3.up, out hit,rayBlockedDistZ))
        {       
        
                if(Global.ghostsColliders.Contains(hit.collider)){
                        Kill(hit.collider);
                }
        }
        
        //verifique se o personagem estah no bloqueado a direita lancando dois raios de cada lado dele
        if (Physics.Raycast(new Vector3(transform.position.x+0.3f,transform.position.y,transform.position.z+0.3f), Vector3.right, out hit,rayBlockedDistX) || 
                Physics.Raycast(new Vector3(transform.position.x-0.3f,transform.position.y,transform.position.z+0.3f), Vector3.right, out hit,rayBlockedDistX))
        {
                if(Global.ghostsColliders.Contains(hit.collider)){
                        Kill(hit.collider);
                }
        }
        
        //verifique se o personagem estah no bloqueado a esquerda lancando dois raios de cada lado dele
        if (Physics.Raycast(new Vector3(transform.position.x+0.3f,transform.position.y,transform.position.z+0.3f), -Vector3.right, out hit,rayBlockedDistX) || 
                Physics.Raycast(new Vector3(transform.position.x-0.3f,transform.position.y,transform.position.z+0.3f), -Vector3.right, out hit,rayBlockedDistX))
        {
                if(Global.ghostsColliders.Contains(hit.collider)){
                        Kill(hit.collider);
                }
                
        }
    }
    
	/// <summary>
	/// Destroi o fantasma ou o pacam.
	/// </summary>
	/// <param name='col'>
	/// o colisor contra o qual o pacman colidiu
	/// </param>
    protected void Kill(Collider col){
        
		//se o pacman estah invencivel e colidiu contra um fantasma ativo e amendrotado na lista global
		//com os colisores dos fantasmas
        if(Global.UPGRADE && col.gameObject.name.Contains("Ghost") && Global.ghostsColliders.Contains(col)
			&& col.gameObject.renderer.sharedMaterial == Global.FEAR){
                
	        //adicione um novo fantasma a lista de respawn com o nome deste fantasma
			//o timer do fantasma sera o tempo global de respawn mais o momento atual
	        Global.ghostsRespawn.Add(new GhostRespawn(col.gameObject.name, Time.time + Global.RESPAWN_TIME));
	        
			//elimine o colisor do fanstama da lista de controle de colisores dos fantasmas
	        Global.ghostsColliders.Remove(col);
			//destrua o objeto de jogo contra o qual o fantasma colidiu
	        Destroy(col.gameObject);
	        
			//chame eatGhost para calcular a pontuacao
			//o numero de fantasmas no vetor de respawn funciona
			//como um multiplicador com valor maximo 4
	        eatGhost(Global.ghostsRespawn.Count);
	        
	        //encerre a funcao
	        return;
        }
        
		//ative a flag global de que o pacman estah morto
        Global.DEAD_PACMAN = true;
        //destrua o pacman
        Destroy(pacMan);

    }
    
	/// <summary>
	/// acessa o placar para atualizar a pontuacao
	/// </summary>
	/// <param name='numGhosts'>
	/// numero de fantasmas abatidos
	/// </param>
    public override void eatGhost(int numGhosts){
		//multiplique o numero de fantasmas mortos pela pontuacao de um
		//fantasma e adicione ao placar
        base.score += numGhosts * base.ghost;
    }
        
        
}