using UnityEngine;
using System.Collections;
using System;
using System.Globalization;
using System.Threading;

public class mainGUI : MonoBehaviour {
	dados Dados;
	public GUISkin GSkin;
	float w = 0;//tamanho relativo da font
	historicoGUI hGUI;
	mapaGUI MapaGUI;
	//Camera camera;
	AudioSource emissorAlarme;

	public bool hide;

	//rects
	public GUIButton[] bot;
	public GUITexto[] texto;
	public GUITexto[] campos;
	public GUICheck alarme;
	public GUIScroll[] scrollVol;

	//dados para a tabela de intervalos
	int X = 0;
	int Y = 0;
	bool visualizando = false;
	public string senha;//senha para habilitar o botÃ£o "atualizar"

	// Use this for initialization
	void Start () {
		Dados = GetComponent<dados>();
		hGUI = GetComponent<historicoGUI>();
		MapaGUI = GetComponent<mapaGUI>();
		//camera = GetComponent<Camera>();
		emissorAlarme = GetComponent<AudioSource>();

		Thread.CurrentThread.CurrentCulture = new CultureInfo("pt-BR");
		//definindo a string do dia
		campos[4].texto = DateTime.Now.Year.ToString()+string.Format("{0:0#.#}", DateTime.Now.Month)+string.Format("{0:0#.#}", DateTime.Now.Day);
		//nome do arquivo a ser salvo do historico
		Dados.saveLoadGeral(3, campos[4].texto);//carregar historico apos definir a string do dia

		//opÃ§Ãµes vÃªm abertas
		Dados.Usuario = usuario.option;
	}

	void OnGUI () {
		if(hide)return;
		GUI.skin = GSkin;
		if(w != Screen.height*0.02f){
			w = Screen.height*0.02f;
		}
		GUI.skin.label.fontSize = (int)(w*1.2);
		GUI.skin.textField.fontSize = (int)(w*1.5);
		GUI.skin.button.fontSize = (int)(w*1.7);
		GUI.skin.toggle.fontSize = (int)(w*1.2);

		//botÃµes
			//botÃ£o embarque
		if(bot[0].desenha()){
			embarque(4);
		}
			//botÃ£o historico
		if(bot[1].desenha()){
			hGUI.hide = false;
			hide = true;
		}
			//botÃ£o "carregar eventos"; editor
		if(Dados.Usuario == usuario.editor)if(bot[2].desenha()){
			try{
				Dados.saveLoadGeral(1);//carregar eventos
			}catch{}
		}
			//botÃ£o "atualizar" tabela de intervalos; editor
		if(Dados.Usuario == usuario.editor)if(bot[3].desenha()){
			try{
				Dados.intervalos[X,Y] = int.Parse(campos[3].texto);
				Dados.saveLoadGeral(6);//salvar intervalos
			}catch{
				campos[3].texto = "0";
			}
		}
			//botÃ£o "limpar eventos"; editor
		if(Dados.Usuario == usuario.editor)if(bot[4].desenha()){
			Dados.EventoD = new eventoD[0];
		}
			//botÃ£o DV
		if(bot[5].desenha()){
			embarque(0);
		}
			//botÃ£o CD
		if(bot[6].desenha()){
			embarque(1);
		}
			//botÃ£o DL
		if(bot[7].desenha()){
			embarque(2);
		}
			//botÃ£o Outro
		if(bot[8].desenha()){
			embarque(3);
		}
			//botÃ£o Options
		if(bot[9].desenha()){
			if(Dados.Usuario == usuario.operador){
				Dados.Usuario = usuario.option;
			}else Dados.Usuario = usuario.operador;
		}

		//textos
		int x = 0;
		while(x<4){
			texto[x].desenha();
			x++;
		}
			//texto do dia de hoje
		if(Dados.Usuario != usuario.editor){//se o usuario nÃ£o for editor o campo do dia de hoje se torna um texto
			x=4;
			campos[x].desenha();
		}

		//campos
		x=0;
		while(x<1){
			campos[x].desenhaField();
			x++;
		}

		//pegando tempo personalizado do campo, agora em minutos
		try{
			float tempo = float.Parse(campos[0].texto);
			Dados.absolutoPersonalizado = tempo*60;
			//ultimo texto[6] Ã© o tempo personalizado padronizado para minutos e segundos
			string segundos;
			segundos = ((int)((tempo - (int)tempo)*60)).ToString();
			x = (int)tempo;
			texto[2].texto = x.ToString()+" minutos "+segundos.ToString()+" segundos";
		}catch{
			campos[0].texto = "10";
		}

		//elementos para usuÃ¡rio nÃ£o operador
		if(Dados.Usuario != usuario.operador){
			
			if(Dados.Usuario == usuario.editor){
				//textos para usuario editor
				x=4;
				while(x<8){
					texto[x].desenha();
					x++;
				}

				//campos de ediÃ§Ã£o da tabela de intervalos
				x=1;
				while(x<5){
					campos[x].desenhaField();
					x++;
				}
			}
			//campo do password
			x=5;
			campos[x].desenhaPassword();
		
			//validaÃ§Ã£o da senha
			if(campos[5].texto == senha){
				Dados.Usuario = usuario.editor;
			}

			//editor da tabela de intervalos
				//origem
			try{
				if(X != int.Parse(campos[1].texto)){
					visualizando = true;
					X = int.Parse(campos[1].texto);
					campos[1].texto = X.ToString();
				}
			}catch{
				campos[1].texto = "0";
			}
			if(X>33||campos[1].texto == ""){
				campos[1].texto = "0";
				X=0;
			}
				//destino
			try{
				if(Y != int.Parse(campos[2].texto)){
					visualizando = true;
					Y = int.Parse(campos[2].texto);
					campos[2].texto = Y.ToString();
				}
			}catch{
				campos[2].texto = "0";
			}
			if(Y>33||campos[2].texto == ""){
				campos[2].texto = "0";
				Y=0;
			}
				//se estiver no modo visualizando
			if(visualizando){//faz o campo do tempo mostrar o valor atual das coordenadas de origem e destino alteradas
				campos[3].texto = Dados.intervalos[X,Y].ToString();
				visualizando = false;
			}
			try{//tenta verificar se Ã© inteiro o valor do campo de intervalo, nÃ£o dando certo, ele serÃ¡ zerado
				int.Parse(campos[3].texto);
			}catch{
				campos[3].texto = "0";
			}
			//fim do editor da tabela de intervalos
			
			//controle geral de ativar ou desativar alarme sonoro e cores de fundo e fonte da letra
				//alarme.onOff = alarme.desenha();
					//alarme.desenha();
					//Dados.alarme = alarme.onOff;
			Dados.alarme = alarme.desenha();
			x=0;
			while(x<scrollVol.Length){
				scrollVol[x].desenhaHorizontal();
				x++;
			}
			emissorAlarme.volume = (float)scrollVol[0].valor/10;

			Color cor = new Color();
			cor.a = 1;
			cor.r = scrollVol[1].valor/10;
			cor.g = scrollVol[2].valor/10;
			cor.b = scrollVol[3].valor/10;
			GetComponent<Camera>().backgroundColor = cor;
			cor.r = scrollVol[4].valor/10;
			cor.g = scrollVol[5].valor/10;
			cor.b = scrollVol[6].valor/10;
			GUI.skin.label.normal.textColor = cor;
			GUI.skin.toggle.hover.textColor = cor;
			GUI.skin.toggle.onHover.textColor = cor;
			GUI.skin.button.hover.textColor = cor;
			GUI.skin.textField.hover.textColor = cor;
			cor.r *= 0.9f;
			cor.g *= 0.9f;
			cor.b *= 0.9f;
			GUI.skin.toggle.normal.textColor = cor;
			GUI.skin.toggle.onNormal.textColor = cor;
			GUI.skin.button.normal.textColor = cor;
			GUI.skin.textField.normal.textColor = cor;
		}
	}

	void embarque (int comandID){
		Dados.relocaEventoDin();
		Dados.rumo = comportamento.novo;
		MapaGUI.hide = false;
		MapaGUI.Origem = true;

		//aquisiÃ§Ã£o da data do windows e armazena como string no novo evento
		Dados.eventoDin.horaEmbarque = null;
		Dados.eventoDin.horaEmbarque = Dados.horaAgora();

		switch (comandID){
			case 0://DV
				Dados.eventoDin.tipo = deficiencia.DV;
				break;
			case 1://CD
				Dados.eventoDin.tipo = deficiencia.CD;
				break;
			case 2://DL
				Dados.eventoDin.tipo = deficiencia.DL;
				break;
			case 3://outro
				Dados.eventoDin.tipo = deficiencia.OUTR;
				break;
			case 4://nulo
				Dados.eventoDin.tipo = deficiencia.nulo;
				break;
			default:
				break;
		}

		hide = true;
	}
}
