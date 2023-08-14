using UnityEngine;
using System.Collections;

public class tipoTremGUI : MonoBehaviour {
	dados Dados;
	public GUISkin GSkin;
	float w = 0;//tamanho relativo da font
	mapaGUI MapaGUI;
	funcGUI FuncGUI;

	//rects
	public GUIButton[] bot;
	public GUITexto[] text;

	public bool hide;

	void Start () {
		Dados = GetComponent<dados>();
		FuncGUI = GetComponent<funcGUI>();
		MapaGUI = GetComponent<mapaGUI>();
	}

	void OnGUI(){
		if(hide)return;
		GUI.skin = GSkin;
		if(w!=Screen.height*0.02f)w = Screen.height*0.02f;
		GUI.skin.label.fontSize = (int)(w*1.1);
		GUI.skin.textField.fontSize = (int)(w*2.0);
		GUI.skin.button.fontSize = (int)(w*1.8);

		int x = 1;//x = 0 para desenhar a box principal
		/*while(x<box.Length){
			GUI.Box(box[x].ajuste(),"");
			x++;
		}*/
		x = 0;
		int y = 100;
		while(x<bot.Length){
			if(bot[x].desenha()){
				y=x;
			}
			x++;
		}
		switch(y){
			case 0://voltar
				hide = true;
				FuncGUI.hide = false;
				FuncGUI.origem = true;
				break;

			case 1://DV
				Dados.eventoDin.tipo = deficiencia.DV;
				avancar();
				break;
			case 2://CD
				Dados.eventoDin.tipo = deficiencia.CD;
				avancar();
				break;
			case 3://DL
				Dados.eventoDin.tipo = deficiencia.DL;
				avancar();
				break;
			case 4://Outro
				Dados.eventoDin.tipo = deficiencia.OUTR;
				avancar();
				break;
			case 5://botão de avançar
				avancar();
				break;

			//tecado numérico na tela
			case 6://tecla 1
				text[4].texto += "1";
				break;
			case 7://tecla 2
				text[4].texto += "2";
				break;
			case 8://tecla 3
				text[4].texto += "3";
				break;
			case 9://tecla 4
				text[4].texto += "4";
				break;
			case 10://tecla 5
				text[4].texto += "5";
				break;
			case 11://tecla 6
				text[4].texto += "6";
				break;
			case 12://tecla 7
				text[4].texto += "7";
				break;
			case 13://tecla 8
				text[4].texto += "8";
				break;
			case 14://tecla 9
				text[4].texto += "9";
				break;
			case 15://tecla 0
				text[4].texto += "0";
				break;
			case 16://tecla LIMPAR
				text[5].texto = "";
				break;

			default:
				break;
		}

		x = 0;
		while(x<4){
			text[x].desenha();
			x++;
		}
		//texto que mostra a informação que está atualmente armazenada no evento ou historico em questão
		text[3].texto = Dados.eventoDin.tipo.ToString()+"|"+Dados.eventoDin.tremTexto();
		//campo do numero do trem
		text[4].desenhaField();
		//tenta pegar a informação do trem para Dados
		try{
			Dados.eventoDin.tremNumero = text[4].texto;
			int.Parse(text[4].texto);
		}catch{
			limpaTrem();
		}
		//se a informação do trem tiver mais de 2 caracteres ela será limpa
		if(Dados.eventoDin.tremNumero.Length>2){
			limpaTrem();
		}

		text[5].desenhaArea();
		Dados.eventoDin.observacao = text[5].texto;
	}

	void limpaTrem(){
		text[4].texto = "";
		Dados.eventoDin.tremNumero = "";
	}

	void avancar(){
		MapaGUI.hide = false;
		MapaGUI.Origem = false;
		hide = true;
	}
}
