using UnityEngine;
using System.Collections;
using System;

public class GUImaster : MonoBehaviour {
	public bool[] windowID;

	public int giveMeID(){
		int count = 0;
		while(windowID[count]!=false){
			count++;
		}
		windowID[count] = true;
		return count;
	}

	public void releaseMyID(int count){
		windowID[count] = false;
	}

	GameObject jogador;

	void Start(){
		jogador = GameObject.FindWithTag("Player");
	}

	public bool playerNear(float alcance, GameObject sensor){
		float distancia=(sensor.transform.position - jogador.transform.position).magnitude;
		return(distancia<alcance);
	}
}

[Serializable ()]
public class GUIRect {
	//posição relativa onde 0 é o mínimo e 1 é o máximo
	public float PositionX;
	public float PositionY;
	public float TamanhoX;
	public float TamanhoY;
	//posição final em pixels
	int posX;
	int posY;
	int tamX;
	int tamY;
	public void recebe (float PX, float PY, float TX, float TY) {
		PositionX = PX;
		PositionY = PY;
		TamanhoX = TX;
		TamanhoY = TY;
	}
	public Rect ajuste(){
		posX = (int)(PositionX * Screen.width);
		posY = (int)(PositionY * Screen.height);
		tamX = (int)(TamanhoX * Screen.width);
		tamY = (int)(TamanhoY * Screen.height);
		Rect retorno = new Rect(posX, posY, tamX, tamY);
		return retorno;
	}
	
	//ajuste dinâmico de posição em X e Y
	public Rect ajuste(float xDin, float yDin){
		posX = (int)((PositionX + xDin) * Screen.width);
		posY = (int)((PositionY + yDin) * Screen.height);
		tamX = (int)(TamanhoX * Screen.width);
		tamY = (int)(TamanhoY * Screen.height);
		return new Rect(posX, posY, tamX, tamY);
	}

	//reposiciona o rect relativo baseado na posição do mouse
	public void reposit(){
		PositionX = Input.mousePosition.x/Screen.width;
		PositionX-= TamanhoX/2;
		PositionX = Mathf.Min((1-TamanhoX), Mathf.Max(PositionX, 0));
		PositionY = 1-Input.mousePosition.y/Screen.height;
		PositionY-= TamanhoY/2;
		PositionY = Mathf.Min((1-TamanhoY), Mathf.Max(PositionY, 0));
	}
}

[Serializable ()]
public class GUIJanela : GUIRect{
	public string nomeDaJanela;
	public int ID;
	public void validate(GUImaster GM){
		ID = GM.giveMeID();
	}
	public void leave(GUImaster GM){
		GM.releaseMyID(ID);
	}
}

[Serializable ()]
public class GUIButton : GUIRect{
	public GUIContent conteudo;
	public bool desenha(){
		return GUI.Button(ajuste(), conteudo);
	}
	public bool desenha(float x, float y){
		return GUI.Button(ajuste(x, y), conteudo);
	}
}

[Serializable ()]
public class GUIImagem : GUIRect{
	public Texture imagem;
	public ScaleMode scalemode;
	public bool transparente;
	public float ratio;
	public void desenha(){
		if(imagem!=null)GUI.DrawTexture(ajuste(),imagem, scalemode, transparente, ratio);
	}
	public void desenha(float x, float y){
		if(imagem!=null)GUI.DrawTexture(ajuste(x, y),imagem, scalemode, transparente, ratio);
	}
}

[Serializable ()]
public class GUITexto : GUIRect {
	public string texto;
	public void desenha(){
		GUI.Label(ajuste(), texto);
	}
	//função para desenho de label
	public void desenha(float x, float y){
		GUI.Label(ajuste(x, y), texto);
	}
	//função para desenho de TextField
	public string desenhaField(){
		texto = GUI.TextField(ajuste(), texto); 
		return texto;
	}
	public string desenhaField(float x, float y){
		texto = GUI.TextField(ajuste(x, y), texto); 
		return texto;
	}
	//função para desenho de TextArea
	public string desenhaArea(){
		texto = GUI.TextArea(ajuste(), texto); 
		return texto;
	}
	public string desenhaArea(float x, float y){
		texto = GUI.TextArea(ajuste(x, y), texto); 
		return texto;
	}
	//função para desenho de PasswordField
	public string desenhaPassword(){
		texto = GUI.PasswordField(ajuste(), texto, '*');
		return texto;
	}
	public string desenhaPassword(float x, float y){
		texto = GUI.PasswordField(ajuste(x, y), texto, '*'); 
		return texto;
	}
}

[Serializable ()]
public class GUICheck : GUIRect {
	public string texto;
	public bool onOff;
	public bool desenha(){
		onOff = GUI.Toggle(ajuste(), onOff, texto);
		return onOff;
	}
	public bool desenha(float x, float y){
		onOff = GUI.Toggle(ajuste(x, y), onOff, texto);
		return onOff;
	}
}

[Serializable ()]
public class GUIScroll : GUIRect {
	public float valor;
	public float visibilidade;
	public float topValor;
	public float bottonValor;
	public void desenhaVertical(){
		valor = GUI.VerticalScrollbar(ajuste(), valor, visibilidade, topValor, bottonValor);
	}
	public void desenhaHorizontal(){
		valor = GUI.HorizontalScrollbar(ajuste(), valor, visibilidade, topValor, bottonValor);
	}
}