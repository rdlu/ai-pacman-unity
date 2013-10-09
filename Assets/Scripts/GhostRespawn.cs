using UnityEngine;
using UnityEditor;
using System.Collections;

/// <summary>
/// Classe que permite o respawn dos fantasmas.
/// </summary>
public class GhostRespawn
{
	//timer para o respawn desse fantasma
	public float timer;
	
	//nome do objeto desse fantasma
	public string ghost;
	
	/// <summary>
	/// Inicializa uma nova instancia dessa classe.
	/// </summary>
	/// <param name='ghost'>
	/// o nome do objeto fantasma.
	/// </param>
	/// <param name='timer'>
	/// o momento de respawn desse fantasma.
	/// </param>
	public GhostRespawn (string ghost, float timer)
	{
		//atribua os valores aos atributos
		this.ghost = ghost;
		this.timer = timer;
	}
}

