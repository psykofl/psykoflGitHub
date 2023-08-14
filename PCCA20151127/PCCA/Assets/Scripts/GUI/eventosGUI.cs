using UnityEngine;
using System.Collections;
using System;

public class eventosGUI : MonoBehaviour {
	public dados Dados;
	public mainGUI MainGUI;
	public mapaGUI MapaGUI;
	public funcGUI FuncGUI;
	public tipoTremGUI TipoTremGUI;
	public historicoGUI HistoricoGUI;
	
	public GUISkin GSkin;
	float y = 0; //fator de tamanho de font

	//rects
	public GUIRect grupo;
	public GUIScroll scrollVert;

	public linhaEvento[] evento;
	public linhaEvento example;

	// Use this for initialization
	void Start () {
		Dados = gameObject.GetComponent<dados>();
		MainGUI = GetComponent<mainGUI>();
		MapaGUI = GetComponent<mapaGUI>();
		FuncGUI = GetComponent<funcGUI>();
		TipoTremGUI = GetComponent<tipoTremGUI>();
		HistoricoGUI = GetComponent<historicoGUI>();
	}

	void OnGUI () {
		GUI.skin = GSkin;
		if(y != Screen.height*0.02f){
			y = Screen.height*0.02f;
		}
		GUI.skin.label.fontSize = (int)y;
		GUI.skin.button.fontSize = (int)y;

		//GUI.Box(box.ajuste(),"");
		GUI.BeginGroup(grupo.ajuste());
		//resize do botton value da scrollBar baseado na quantidade de itens do historico
		if(scrollVert.bottonValor != evento.Length){
			scrollVert.bottonValor = evento.Length;
		}
		//parte que esconde ou mostra a barra de rolagem lateral para quando houver muitos itens
		if(scrollVert.bottonValor>scrollVert.visibilidade){
			scrollVert.desenhaVertical();
		}else scrollVert.valor = 0;

		//parte que equaliza a quantidade de linha com a quantidade de eventos
		int x = 0;
		if(evento.Length != Dados.EventoD.Length){
			evento = new linhaEvento[Dados.EventoD.Length];
			while(x<evento.Length){
				evento[x] = (linhaEvento)example.clone(Dados);
				x++;
			}
		}
		//parte que navega em todo o vetor para coletar e desenhar linha a linha
		x = 0;
		while(x<evento.Length){
			evento[x].pegaDados(Dados.EventoD[x]);
			evento[x].desenha(scrollVert.valor, x, this);
			x++;
		}

		//Parte para teste e edição da linha padrão "example"
		//example.pegaDados(Dados.EventoD[0]);
		//example.desenha(scrollVert.valor, 0);

		GUI.EndGroup();
	}
}

//definição da classe de linha de evento
[Serializable ()]
public class linhaEvento{
	public GUITexto[] palavra;
	public GUIButton[] bot;
	public GUICheck silencioso;

	public dados Dados;

	//construtor padrão
	public linhaEvento(){
		palavra = new GUITexto[18];
		int x = 0;
		while(x<palavra.Length){
			palavra[x] = new GUITexto();
			x++;
		}
		bot = new GUIButton[4];
		x=0;
		while(x<bot.Length){
			bot[x] = new GUIButton();
			x++;
		}
		silencioso = new GUICheck();
	}

	//função que coleta os dados para a linha que será desenhada
	public void pegaDados(eventoD evento){
		palavra[0].texto = evento.absolutoTexto();
		palavra[2].texto = (string)evento.horaEmbarque;
		palavra[4].texto = (string)evento.estOrigem.nome;
		palavra[6].texto = (string)evento.respEmbarque.nome;
		palavra[8].texto = (string)evento.tipo.ToString();
		palavra[10].texto = (string)evento.tremTexto();
		palavra[12].texto = (string)evento.estDestino.nome;
		palavra[14].texto = (string)evento.respDesembarque.nome;
		palavra[16].texto = (string)evento.horaDesembarque;
		palavra[17].texto = (string)evento.observacao;
	}

	//método que escreve a linha na box
	public void desenha(float scrollValor, int y, eventosGUI EventosGUI){
		int x = 0;
		while(x<palavra.Length){
			palavra[x].desenha(0, (-scrollValor+y)/14);//método desenha dinâmico recebendo valores relativos a posição e alteração baseada na scrollbar
			x++;
		}
		//botão "posição"
		if(bot[0].desenha(0, (-scrollValor+y)/14)){
			EventosGUI.MainGUI.hide = true;
			EventosGUI.HistoricoGUI.hide = true;
			EventosGUI.FuncGUI.hide = true;
			EventosGUI.TipoTremGUI.hide = true;
			EventosGUI.MapaGUI.hide = false;
			EventosGUI.MapaGUI.Origem = true;
			EventosGUI.Dados.rumo = comportamento.posit;
			EventosGUI.Dados.eventoDin = EventosGUI.Dados.EventoD[y];
		}
		//botão "informar"
		if(bot[1].desenha(0, (-scrollValor+y)/14)){
			EventosGUI.MainGUI.hide = true;
			EventosGUI.HistoricoGUI.hide = true;
			EventosGUI.MapaGUI.hide = true;
			EventosGUI.TipoTremGUI.hide = true;
			EventosGUI.FuncGUI.hide = false;
			EventosGUI.FuncGUI.text[4].texto = "";
			EventosGUI.Dados.rumo = comportamento.infor;
			EventosGUI.FuncGUI.origem = false;
			EventosGUI.Dados.eventoDin = EventosGUI.Dados.EventoD[y];
		}
		//botão "desceu"
		if(bot[2].desenha(0, (-scrollValor+y)/14)){
			EventosGUI.Dados.EventoD[y].ativo = false;
		}
		//botão "editar"
		if(bot[3].desenha(0, (-scrollValor+y)/14)){
			EventosGUI.MainGUI.hide = true;
			EventosGUI.HistoricoGUI.hide = true;
			EventosGUI.FuncGUI.hide = true;
			EventosGUI.TipoTremGUI.hide = true;
			EventosGUI.MapaGUI.hide = false;
			EventosGUI.MapaGUI.Origem = true;
			EventosGUI.TipoTremGUI.text[4].texto = EventosGUI.Dados.EventoD[y].tremNumero.ToString();
			EventosGUI.TipoTremGUI.text[5].texto = EventosGUI.Dados.EventoD[y].observacao;
			EventosGUI.Dados.rumo = comportamento.Editar;
			EventosGUI.Dados.eventoDin = EventosGUI.Dados.EventoD[y];
		}
		silencioso.onOff = Dados.EventoD[y].silenciado;
		Dados.EventoD[y].silenciado = silencioso.desenha(0, (-scrollValor+y)/14);
	}

	public object clone(dados dadosR){
		Dados = dadosR;
		return this.MemberwiseClone();
	}
}