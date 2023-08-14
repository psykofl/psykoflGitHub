using UnityEngine;
using System.Collections;

public class trava : MonoBehaviour {
	public bool travado = false;
	public string dia;
	public string mes;
	public string ano;
	public int dias;

	// Use this for initialization
	void Start () {
		//tenta carregar o arquivo validador e atribui seu valor interno à trava
		try{
			travado = (bool)SaveLoad.Load(@"C:\Users\Public\val.vld");
		}catch{//se não der certo o "load" a trava é acionada
			travado = true;
		}
	}
	
	// Update is called once per frame
	void Update () {
		//se a trava estiver acionada, o programa trava ao pressionar qualquer tecla ou botão do mouse
		if(Input.anyKey){
			if(travado)Application.LoadLevel("vazio");
		}
	}

	//função de verificação de validade
	public void validade(){
		int y = 0;//contador de dias que se passaram a partir do começo de 2012
		//tenta converter a data para numero de dias, considerando ano de 2012 como ano zero
		try{
			y = int.Parse(dia)+((int.Parse(mes)-1)*30)+((int.Parse(ano)-2012)*365);
		}catch{//se a conversão não der certo a trava é acionada, para o caso de computador sem hora
			travado = true;
		}
		//se o numero atual de dias "y" for maior do q a validade "dias", a trava é acionada e salva
		if(y>dias||y<270){
			travado = true;
			SaveLoad.Save(@"C:\Users\Public\val.vld", travado);
		}
	}
}