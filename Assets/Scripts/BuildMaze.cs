using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Classe que constroi o labirinto.
/// </summary>
public class BuildMaze : MonoBehaviour {
	
	
	
	//os elementos abaixo sao vinculador a partir do Inspector do Unity
	//verifique a atribuicao clicando em BondaryMap no ambiente do Unity
	//arquivo de texto com o cenario de jogo
	public TextAsset mazeFile;
	//bloco do labirinto	
	public Transform blockPrefab;
	//pelotas
	public Transform pelletPrefab;
	//pelotas grandes
	public Transform superPrefab;
	//lista estatica com os nos do cenario. 
	//Pode ser acessada diretamente por outras classes
	public List<PathNode> nodes;
	
	/// Chamada na inicializacao da classe pelo unity atraves da classe MonoBehaviour. 
    /// Inicia a instancia dessa classe.
    /// </summary>
	void Start () {
	
		//cria um array contendo cada linha do cenario
		string[] map = mazeFile.text.Split("\n"[0]);
		//instancia um vetor de 3 coordenadas
   	 	Vector3 coord = new Vector3 ();
		//instancia a lista de nos.
		nodes = new List<PathNode>();
		
		//trave o vetor de coordenadas no eixo y
		//ocenario eh construido ao longo dos eixos x e z.
	    coord.y = 1.0f;
		
		//calcule o off set das linhas do mapa
		//representa o tanto que as linhas devem ser deslocadas a esquerda
	    float row_off = map.Length / 2.0f;
	    
		//para cada linha
		for (int row = 0; row < map.Length; row ++) {
			
			//calcule a posicao da coordenada z
	        coord.z = (map.Length - row - row_off - 1) * Global.offSet;
	        
			//calcule o off set das colunas do mapa
			float col_off = map[row].Length / 2.0f;
	        
			//para cada coluna
			for (int col = 0; col < map[row].Length; col ++) {
				//calcule a posicao da coordenada x
	            coord.x = (col - col_off) * Global.offSet + 1;
	           
				//se o caractere nesa posicao for X (parede)
				if (map[row][col] == 'X') {
					//instancie um bloco neste local. Quaternion.identity indica que nao sera aplicada
					//rotacao ao elemento
	                Transform inst = Instantiate (blockPrefab, coord, Quaternion.identity) as Transform;
					
					//indica que o elemento pai do objeto instanciado eh o transform desta classe
					//como a classe esta anexada as paredes que circundam o cenario, eleas seram
					//o objeto pai do bloco instanciado
	                inst.transform.parent = transform;
					
					//instancie um novo no nesta posicao e adicione a lista de nos do cenario
					PathNode node = new PathNode();
					node.Position = coord;
					node.Wall = true;
					node.Cost = float.MaxValue;
					
					nodes.Add(node);	
	            } 
				//se o caractere nesa posicao for . (pelota)
				else if (map[row][col] == '.') {
					
					//instancie uma pelota neste local. nao eh necessario definir as paredes 
					//externas do cenario como seu objeto pai jah que nao estao "presas" a ele
	                Transform inst = Instantiate (pelletPrefab, coord, Quaternion.identity) as Transform;
					
					
					//instancie um novo no nesta posicao e adicione a lista de nos do cenario
					PathNode node = new PathNode();
					node.Position = coord;
					node.Wall = false;
					node.Pellet = inst;
					
					
					nodes.Add(node);
	            } 
				//se o caractere nesa posicao for O (pelota grande)
				else if (map[row][col] == 'O') {
					
					//instancie uma pelota grande neste local.
	                Transform inst = Instantiate (superPrefab, coord, Quaternion.identity) as Transform;
					
					//instancie um novo no nesta posicao e adicione a lista de nos do cenario
					PathNode node = new PathNode();
					node.Position = coord;
					node.Wall = false;
					node.Pellet = inst;
					
					nodes.Add(node);
	            }//se o caractere nesa posicao for O (pelota grande)
				else if (map[row][col] == ' ') {
					
					//instancie uma pelota grande neste local.
	               	// Transform inst = Instantiate (superPrefab, coord, Quaternion.identity) as Transform;
					
					//instancie um novo no nesta posicao e adicione a lista de nos do cenario
					PathNode node = new PathNode();
					node.Position = coord;
					node.Wall = false;
					
					nodes.Add(node);
	            }
	        }
			
	    }
		
		//para cada no do labirinto
		foreach(PathNode node in nodes){
			//habilite caso queira ver a relacao de nos
			//Debug.Log(node.Position + " Parede: " + node.Wall);
			
			//encontre os nos de conexao para cada no do labirinto
			Global.buildConnections(node, nodes, true);
		}
		
		//atribua os nos a variavel de acesso global
		Global.nodes = nodes;
	}
	
	
	
	/// <summary>
    /// Faz o update da classe. A Unity chama update uma vez por frame a
    /// partir da classe (pai) MonoBehaviour
    /// Como o labirinto nao se altera apos a construcao o metodo esta vazio.
    /// </summary>
	void Update () {

	}
}