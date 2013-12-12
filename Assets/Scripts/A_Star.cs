using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Classe do algoritmo A* que controla o movimento atraves do labirinto
/// </summary>
public class A_Star {
	
	//objeto do personagem na unity
	public Transform character;
	
	//indica se este algoritmo esta sendo executado
	//para o jogador
	bool isPlayer;
	
	//estado da localizacao atual do personagem
	PathNode charState;
	
	//proximo estado ao qual o personagem quer chegar
	PathNode nextState;
	
	//lista de nos do cenario. Um grafo com os caminhos e paredes
	List<PathNode> nodes;
	
	//lista de nos do melhor caminho retornardo pelo A*
	List<PathNode> bestPath;
	
	/// <summary>
	/// Inicializa uma nova instancia do A*
	/// </summary>
	/// <param name='character'>
	/// Objeto transforme do personagem, contem informacoes diversas, incluindo sua posicao
	/// </param>
	public A_Star (Transform character, bool isPlayer) {
		
		//atribua o valor do personagem
		this.character = character;
		
		//inicie o estado do personagem
		charState = new PathNode();
		
		this.isPlayer = isPlayer;
		//recupere os nos do labirinto a partir da classe BuildMaze
		//(nem sempre funciona no Start. Pode ocorrer de se a atribuica
		//antes do labirinto ser criando. Eh importante chamar novamente
		//dentro do
		nodes = Global.nodes;
	}
	
	/// <summary>
	/// Encontra os nos correspondendo ao melhor caminho entre o no atual e um no de destino
	/// </summary>
	/// <returns>
	/// O melhor caminho
	/// </returns>
	public List<PathNode> findBestPath() {
		
		//verifique se ha um proximo estado (de destino) valido. Caso nao haja, retorne
		//o no atual
		//obs:
		//cuidado com comparacoes de null. O unity por padrao usa como comparacao para null
		//o campo name do objeto criado. Caso nao tenha sido definido mesmo um objeto lido como
		//null pode ter conteudo, sendo interessante outras checagens.
		if(nextState == null && nextState.Position.x == float.MaxValue && 
			nextState.Position.y == float.MaxValue && nextState.Position.z == float.MaxValue){
			//retorne uma lista que contem apenas o no atual
			return noPathFound();
		}
		
		//atribua o estado atual ao local onde o personagem estah.
		//isto deve ser feito a cada iteracao do algoritmo
		charState.Position = character.transform.position;
		
		//encontre o melhor caminho ateh o proximo estado
		bestPath = this.findBestPath(charState, nextState);

		//retorne o melhor caminho encontrado
		return bestPath;
	}
	
	
	/// <summary>
	/// Encontra os menores caminhos possiveis do no atual (origem) ao destino desejado
	/// </summary>
	/// <returns>
	/// O melhor caminho possivel da origem ao destino desejado
	/// </returns>
	/// <param name='charNode'>
	/// O no com as informacoes que representam o personagem.
	/// </param>
	/// <param name='destinyNode'>
	/// O no com as informacoes que representam o destino desejado.
	/// </param>/
	public List<PathNode> findBestPath(PathNode charNode, PathNode destinyNode){
		
		//lista para o conjunto de nos a ser avaliados
		List<PathNode> openSet = new List<PathNode>();
		//lisa para o conjunto de nos jah avaliados
		List<PathNode> closedSet = new List<PathNode>();
		
		//Lista de tipo dicionario. Armzena todo o percurso tracado pela busca. 
		//vinculando cada passo da busca ao no de onde veio. Isto permite que a
		//informacao do no de destino, apos armazenada possa ser recuperada 
		//recursivamente do destino ao no que a gerou, resultando em uma lista
		//contendo o melhor percurso, que podera ser armazenado em outra lista
		Dictionary<PathNode, PathNode> cameFrom = new Dictionary<PathNode, PathNode>();
		
		//no inicial
		PathNode startNode;
		
		//procure pelo no inicial na lista de nos
		int sIndex = nodes.IndexOf(charNode);
		
		//se nao existe (o no do personagem nao consta na lista de nos. 
		//comum na primeira passagem do algoritmo) e eh o jogador
		if(sIndex < 0 && isPlayer){
			//instancie o no inicial
			startNode = new PathNode();
			//atribua ao no inicial a posicao do personagem
			startNode.Position = charNode.Position;
		}
		//senao, se nao existe e nao eh o jogador
		else if (sIndex < 0 && !isPlayer){
			//encotre o no mais proximo da posicao desejada e atribua
			startNode = Global.findClosestNode(charNode.Position, Global.nodes);
		}
		//senao
		else{
			//atribua esse no ao no inicial
			startNode = nodes[sIndex];
		}
		
		//se o no inicial nao possui conexoes
		//(comum quando o if anterior eh verdadeiro)
		if(startNode.Connections.Count == 0){
			//recupere as conexoes existentes sem forcar a criacao de nos nao encontrados
			Global.buildConnections(startNode, nodes, false);
		}
		
		//calcule a linha reta do no inicial ao destino
		//caso possivel isso eh a melhor rota possivel e, portanto, uma heuristica admissivel
		//ou seja, uma heuristica que nao esta superfaturada
		startNode.Heuristic = Vector3.Distance(startNode.Position, destinyNode.Position);
			
		//adicione o no inicial ao conjunto de nos a serem avaliados
		openSet.Add(startNode);
		
		//instancie uma variavel para armazenar o no atual da busca
		PathNode currentNode = null;
		
		//enquanto houverem nos abertos
		while(openSet.Count > 0){
			
			//encontre o no de custo estimado final mais baixo
			currentNode = findLowerEstimatedCost(openSet/*, destinyNode*/);
			
			//se o no atual for o no de destino
			if(currentNode.Equals(destinyNode)){
				
				//instancie uma nova lista para armazenar o melhor caminho
				List<PathNode> path = new List<PathNode>();
				//defina o caminho, envie a lista por referencia para assegurar
				//que o mesmo objeto seria preenchido e devolvido
				setPath(cameFrom, currentNode, ref path);
				//retorne o melhor caminho
				return path;
			}
			
			//remova o no atual da lista de nos a serem avaliados e 
			//insira-o a lista de nos jah explorados
			openSet.Remove(currentNode);
			closedSet.Add(currentNode);
			
			//na eventualidade do no nao possuir conexoes, encontre-as!
			//Obs.: Era necessario nas primeiras implementacoes do algoritmo.
			//Como o algoritmo esta estavel nao eh mais necessaria, mas eh mantido
			//apenas por extrema precaucao
			if(currentNode.Connections.Count == 0)
				Global.buildConnections(currentNode, nodes, false);
			
			//para cada conexao do no atual
			foreach(PathNode conn in currentNode.Connections){
				
				//se o no jah foi avaliado e estah no conjunto fechado, avance para o proximo no.
				//no caso de ser uma parede (no invalido) tambem avance. Nao eh interessante avaliar as paredes.
				//alem do custo dela ser tao alto que so seriam removidas da lista de nos abertos no caso do algoritmo 
				//esgotar suas possibilidades (em caso de um erro) isso seria ainda mais incorreto visto que nao eh
				//possivel ao personagem alcanca-las ou se mover a partir delas
				if(closedSet.Contains(conn) || conn.Wall)
					continue;
				
				//calcule o custo estimado (heuristica) ateh o destino a partir deste ponto
				float h_conn = Vector3.Distance(conn.Position, destinyNode.Position);
				
				
				//calcule o custo real ateh esse ponto a partir do custo para chegar no no atual somado 
				//a distancia do no atual ateh esse ponto
				float cost_till_here = currentNode.Cost + Vector3.Distance(currentNode.Position, conn.Position);
				
				//avalia se esse caminho possui um custo menor
				bool betterCost = false;
				
				//se esse no nao existe na lista de nos abertos, inclua-o
				if(!openSet.Contains(conn)){
					//adicione o no
					openSet.Add(conn);
					//como o no nao existe, esse eh (ao menos por hora) 
					//o melhor custo ateh aqui
					betterCost = true;
				}
				//se esse no jah existe na lista de nos abertos e o custo 
				//por este novo caminho eh menor que o custo jah avaliado
				else if (cost_till_here < conn.Cost){
					//o custo por esse novo percurso eh melhor
					betterCost = true;
				}
				
				//se esse custo eh melhor
				if(betterCost){
					
					//armazena no dicionario o no da conexao e
					//indica que a sua origem eh o no atual
					cameFrom[conn] = currentNode;
					//atribua o custo por esse caminho ao custo do no da conexao
					conn.Cost = cost_till_here;
					//atribua o custo da heuristica desse no da conexao ao destino desejado
					conn.Heuristic = h_conn;
				}
			}
		}
		
		//se chegou ateh aqui eh porque nao foi possivel tracar um caminho.
		//isto indica que ha algum erro na implementacao dos nos. Verifique
		//a construcao dos nos da posicao inicial (StartNode), suas conexoes
		//e, se necessario, da construcao do labirinto
		return noPathFound();
	}
	
	/// <summary>
	/// Define o melhor caminho armazenado no dicionario, construindo recursivamente a lista de nos da origem 
	/// ao destino a partir do destino
	/// </summary>
	/// <param name='cameFrom'>
	/// Dicionario com os nos explorados e seus caminhos de origem.
	/// </param>
	/// <param name='currentNode'>
	/// O no atual que se estah procurando no dicionario.
	/// </param>
	/// <param name='result'>
	/// O melhor caminho da origem ao destino reconstruido a partir do dicionario de nos
	/// </param>/
	void setPath(Dictionary<PathNode, PathNode> cameFrom, PathNode currentNode, ref List<PathNode> result){
		
		//se o dicionario contem o no atual
		if(cameFrom.ContainsKey(currentNode))
		{
			//chame novamente o metodo, passando como no atual 
			//a origem do no que foi avaliado nesta chamada
			setPath(cameFrom, cameFrom[currentNode], ref result);
			//adicione o no a lista de resultados
			result.Add(currentNode);
			//retorne
			return;
		}
		//adicione o no a lista de resultados
		result.Add(currentNode);
			
	}
	
	/// <summary>
	/// Procura dentro da lista de nos nao avaliados aqueles com o menor custo total estimado
	/// ateh o no de destino (custo para chegar ateh este no + heuristica ateh o destino).
	/// </summary>
	/// <returns>
	/// O no nao avaliado com o menor custo estimado.
	/// </returns>
	/// <param name='openSet'>
	/// O conjunto de nos ainda nao avaliados.
	/// </param>
	PathNode findLowerEstimatedCost(List<PathNode> openSet){
		
		//inicie o indice com zero
		int index = 0;
		//inicie o melhor custo encontrado com o valor maximo possivel
		float lowEC = float.MaxValue;
		
		//para cada no da lista
		for(int i = 0; i < openSet.Count; i++){
			//recupere o no atual
			PathNode aNode = openSet[i];
			
			//recupere o custo estimado desse no
			float estCost = aNode.EstimatedCost;
			
			//se o custo estimado eh menor que o menor custo encontrado
			if(estCost < lowEC){
				lowEC = estCost;
				index = i;
			}
		}
		
		//retorne o no do melhor custo
		return openSet[index];
	}
	
	/// <summary>
	/// Escolhe um estado aleatorio dentro da arvore de nos do labirinto.
	/// </summary>
	/// <returns>
	/// Um no aleatorio que representa um estado do objeto.
	/// </returns>
	public PathNode chooseRandomState(){
		
		//no de retorno
		PathNode nodeD = null;
		
		//faca
		do{
			//recupere os nos do labirinto
			List<PathNode> nodes = Global.nodes;
		
			//escolha ao acaso um indice da arvore de nos do labirinto
			//subtraia um do tamanho (posicoes validas vao d 0 a count-1)
			int index = (int) (Random.value * (nodes.Count - 1)); 
			
			//atribua o no do indice ao no de retorno
			nodeD = nodes[index];
		}
		//enquanto o no de retorno for uma parede
		while(nodeD.Wall);
		
		//retorne o no selecionado
		return nodeD;
	}
	
	/// <summary>
	/// Indica se eh possivel encontrar a posicao atual do personagem dentro da lista de nos do labirinto
	/// </summary>
	/// <returns>
	/// verdadeiro se a posicao do personagem consta na lista de nos do labirinto
	/// </returns>
	/// <param name='character'>
	/// Objeto transform vinculado ao personagem
	/// </param>
	public bool canReset(Transform character){
		
		//instancie um novo no
		PathNode charCurrState = new PathNode();
		
		//atribua oa novo no o local onde o personagem estah
		charCurrState.Position = character.transform.position;
		
		//procure pelo no inicial na lista de nos
		int index = nodes.IndexOf(charCurrState);
		
		//retorne se o indice do personagem na lista de nos eh ou nao maior ou igual a zero
		return index >= 0;
	}
	
	/// <summary>
	/// Nos the path found.
	/// </summary>
	/// <returns>
	/// The path found.
	/// </returns>
	public List<PathNode> noPathFound(){
		
		//exiba uma mensagem indicando que nao foi possivel tracar um caminho
		Debug.Log("Nao foi possivel encontrar um caminho para o personagem. " +
			"Verifique se o no de destino foi definido e/ou as conexoes do no inicial.");
		
		//instancie um lista e inclua o estado atual do personagem
		List<PathNode> noMove = new List<PathNode>();
		noMove.Add(charState);
		
		//retorne a lista
		return noMove;
	}
	
	/// <summary>
	/// Recupera ou define o valor do no do proximo estado.
	/// </summary>
	/// <value>
	/// o valor do proximo estado.
	/// </value>
	public PathNode NextState{
		get{return nextState;}
		set{nextState = value;}
	}
}