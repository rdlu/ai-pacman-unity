using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Armazena informacoes que identificam um no do grafo do labirinto.
/// Herda System.IEquatable e sobrescreve o metodo de igualdade para
/// realizar a comparacao apenas em relacao as coordenadas do no
/// </summary>
public class PathNode : System.IEquatable<PathNode> {

	//indica se o no eh uma parede
	bool isWall;
	
	//custo inicial da heuristica. note que como nossa heuristica eh 
	//a distancia entre dois pontos -1 nunca sera um valor valido
	float heuristic = -1;
	
	//custo real para se alcancar este no
	float cost = 0;
	
	//pelota no ponto do labirinto indicado por este no
	Transform pellet;
	
	//posicao deste no no labirinto
	Vector3 position;
	
	//lista as conexoes deste no com seus 
	//pontos diretamente adjacentes
	List<PathNode> connections;
	
	/// <summary>
	/// Inicializa uma nova instancia de PathNode
	/// </summary>
	public PathNode(){
		
		//instancia a lista de conexoes
		connections = new List<PathNode>();
		//inicializa as pelotas com nulo
		pellet = null;
		
		//atribui ao vetor de posicoes iniciais
		//o valor maximo possivel
		position.x = float.MaxValue;
		position.y = float.MaxValue;
		position.z = float.MaxValue;
	}
	
	/// <summary>
	/// Adiciona um no a lista de conexoes.
	/// </summary>
	/// <param name='conn'>
	/// o no a ser adicionado.
	/// </param>
	public void AddConnnection(PathNode conn){
		
		//se a lista de conexoes eh nula, inicializa
		if(connections == null)
			connections = new List<PathNode>();
		
		//adicione o no a lista de conexoes
		connections.Add(conn);
	}
	
	/// <summary>
	/// Retorna a lista de conexoes.
	/// </summary>
	/// <value>
	/// a lista de conexoes.
	/// </value>
	public List<PathNode> Connections{
		get{return connections;}
		//set{connections = value;}
	}
	
	/// <summary>
	/// Retorna ou define a pelota neste no do cenario.
	/// </summary>
	/// <value>
	/// A pelota.
	/// </value>
	public Transform Pellet{
		get{return pellet;}
		set{
			pellet = value;
		}
	}
	
	/// <summary>
	/// Retorna ou define o valor que indica se este no eh uma parede.
	/// </summary>
	/// <value>
	/// <c>true</c> se eh uma parede; caso contrario, <c>false</c>.
	/// </value>
	public bool Wall{
		get{return isWall;}
		set{
			isWall = value;
		}
	}
	
	/// <summary>
	/// Retorna ou define a posicao deste no no cenario.
	/// </summary>
	/// <value>
	/// A posicao.
	/// </value>
	public Vector3 Position{
		get{return position;}
		set{
			position = value;
		}
	}
	
	/// <summary>
	/// Retorna ou define o custo real para se alcancar este no.
	/// </summary>
	/// <value>
	/// The cost.
	/// </value>
	public float Cost{
		get{return cost;}
		set{
			cost = value;
		}
	}
	
	/// <summary>
	/// Retorna ou define o custo da heuristica deste no ateh o destino desejado
	/// </summary>
	/// <value>
	/// The heuristic.
	/// </value>
	public float Heuristic{
		get{return heuristic;}
		set{heuristic = value;}
	}
	
	/// <summary>
	/// Retorna custo estimado do percurso: a soma do custo ateh aqui com a 
	/// heuristica do custo deste no ao destino desejado
	/// </summary>
	/// <value>
	/// O custo estimado.
	/// </value>
	public float EstimatedCost{
		get{return cost + heuristic;}
	}
	
	/// <summary>
	/// Calcula o custo da linha reta deste no ateh o destino desejado.
	/// </summary>
	/// <returns>
	/// O custo estimado.
	/// </returns>
	/// <param name='destiny'>
	/// A posicao de destino desejada
	/// </param>
	public float EstimatedCostToDestiny(Vector3 destiny){
		
		return Vector3.Distance(position, destiny);
	}
	
	/// <summary>
	/// Determina se o no especificado eh igual a este no
	/// </summary>
	/// <param name='other'>
	/// O no a ser comparado com este no
	/// </param>
	/// <returns>
	/// <c>true</c> se as coordenadas do no especificado sao iguais as deste no
	/// </returns>
	public bool Equals(PathNode other){
		
		//recupere as coordenadas x, y e z do no especificado
		float x = other.Position.x;
		//float y = other.Position.y;
		float z = other.Position.z;
		
		//retorne se as tres coordenadas sao ou nao iguais a deste no
		return position.x == x /*&& position.y == y */&& position.z == z;
		
		
	}
	
	//Procura o Pellet mais proximo deste no usando BFS
	public PathNode bfs_pellet() {
		Queue<PathNode> queue = new Queue<PathNode>();
		HashSet<PathNode> V = new HashSet<PathNode>();
		
		queue.Enqueue(this); V.Add(this);
		while(queue.Count > 0) {
			PathNode t = queue.Dequeue();
			if(t.Pellet != null) {
				return t;
			}
			foreach(PathNode connection in this.Connections) {
				if(!V.Contains(connection)) { //is not in set V
					V.Add(connection);
					queue.Enqueue(connection);
				}
			}
		}
		return null;
	}
}
