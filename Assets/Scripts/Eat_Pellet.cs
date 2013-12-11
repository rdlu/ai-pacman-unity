using UnityEngine;
using System.Collections;

/// <summary>
/// Classe que permite ao PacMan comer pelotas.
/// </summary>
public class Eat_Pellet : MonoBehaviour {
	
	//objeto com o texto da pontuacao (atribuido no Unity Inspector)
	public GUIText scoreDisplay;
	
	//valor da pontuacao de cada pelota, pelota grande e fantasma
	//podem ser ajustados no codigo ou no Unity Inspector
	//o valor do Inspector tem sempre prioridade e 
	//eh o que eh de fato aplicado
	public int smallPelletScore = 10;
	public int superPelletScore = 100;
	public int ghost = 200;
	
	//pontuacao total
	protected int score;
	
	/// <summary>
	/// Atualiza a pontuacao.
	/// </summary>
	public void UpdateScore () {
		
		//se o display da pontuacao nao estah atribuido
		if(scoreDisplay == null){
			//encontre o objeto GUIText e atribua-o a esta variavel
			scoreDisplay = (GUIText) GameObject.FindObjectOfType(typeof (GUIText));
			//descomente se necessario
			//Debug.Log(scoreDisplay.name);
		}
		
		//atualize o texto
		scoreDisplay.text = "Pontos: " + score;
	}
	
	/// <summary>
	/// Ativado durante o evento TriggerEnter.
	/// Perceba que no Unity Inspector os objetos 
	/// de pelotas estao marcados como triggers (gatilhos), 
	/// o que permite o uso de tais metodos
	/// </summary>
	/// <param name='other'>
	/// O objeto com o qual houve a colisao.
	/// </param>
	void OnTriggerEnter (Collider other) {
		Debug.Log("TRIGGER ERRADO");
		//se o objeto era uma pelota
	    if (other.name.Contains("BasicPellet")) {
	        score += smallPelletScore;
	    } 
		//senao, se o objeto era uma superpelota
		else if (other.name.Contains("SuperPellet")) {
	        score += superPelletScore;
			
			//ative a invencibilidade do pacman
			Global.UPGRADE = true;
	    }
		
		//destrua o objeto deste colisor
	    Destroy(other.gameObject);
	}
	
	/// <summary>
	/// Contabiliza a captura dos fantasmas. 
	/// Implementada na classe filha (PacMan)
	/// </summary>
	/// <param name='numGhosts'>
	/// Numero de fantasmas capturados.
	/// </param>
	public virtual void eatGhost(int numGhosts){}
}
