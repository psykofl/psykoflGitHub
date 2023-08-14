using UnityEngine;
using System.Collections;
using System;

public class historicoGUI: MonoBehaviour {
	public dados Dados;
	public mainGUI MainGUI;
	public mapaGUI MapaGUI;
	public funcGUI FuncGUI;
	public GUISkin GSkin;
	float y = 0;//tamanho relativo da font
	public bool hide;

	//rects
	public GUIButton[] bot;
	public GUITexto[] texto;

	public GUIRect histGrupo;
	public GUIScroll histScrollVert;

	public GUIHistorico[] historico;
	bool editaveis;
	public GUIHistorico example;

	// Use this for initialization
	void Start () {
		Dados = gameObject.GetComponent<dados>();
		MainGUI = GetComponent<mainGUI>();
		MapaGUI = GetComponent<mapaGUI>();
		FuncGUI = GetComponent<funcGUI>();

		editaveis = false;
	}

	void OnGUI () {
		if(hide)return;
		GUI.skin = GSkin;
		if(y != Screen.height*0.02f){
			y = Screen.height*0.02f;
		}
		GUI.skin.button.fontSize = (int)(y*1.5f);
		GUI.skin.label.fontSize = (int)y;

		//GUI.Box(histBox.ajuste(),"");
		GUI.BeginGroup(histGrupo.ajuste());
		//resize do botton value da scrollBar baseado na quantidade de itens do historico
		if(histScrollVert.bottonValor != historico.Length){
			histScrollVert.bottonValor = historico.Length;
		}
		//parte que esconde ou mostra a barra de rolagem lateral para quando houver muitos itens
		if(histScrollVert.bottonValor>histScrollVert.visibilidade){
			histScrollVert.desenhaVertical();
		}else histScrollVert.valor = 0;

		//parte que escreve as linhas
			//parte que equaliza a quantidade de linha com a quantidade de historicos
		int x = 0;
		if(historico.Length != Dados.Historico.Length){
			historico = new GUIHistorico[Dados.Historico.Length];
			while(x<historico.Length){
				historico[x] = (GUIHistorico)example.clone();
				x++;
			}
		}
			//parte que navega em todo o vetor para coletar e desenhar linha a linha
		x = 0;
		while(x<Dados.Historico.Length){
			historico[x].pegaDados(Dados.Historico[x]);
			historico[x].desenha(histScrollVert.valor, x, this, editaveis);
			x++;
		}

		//Parte para teste e edição da linha padrão "example"
		//example.desenha(histScrollVert.valor, 0);
		
		GUI.EndGroup();

		//botão "voltar"
		if(bot[0].desenha()){
			hide = true;
			MainGUI.hide = false;
		}
		//botão "carregar"
		if(Dados.Usuario == usuario.editor)if(bot[1].desenha()){
			Dados.saveLoadGeral(3, MainGUI.campos[4].texto);//carregar historico
		}
		//botão "limpar"
		if(Dados.Usuario == usuario.editor)if(bot[2].desenha()){
			Dados.Historico = new historico[0];
		}
		//botão "editar"
		if(bot[3].desenha()){
			editaveis = !editaveis;
		}
		//botão "gerar txt"
		if(bot[4].desenha()){
			string[] bloco = new string[Dados.Historico.Length+1];
			bloco[0] = "Relatorio retirado no dia "+Dados.dataOriginal()+". Ocorreram "+Dados.Historico.Length.ToString()+" acompanhamentos.";
			x=1;
			while(x<bloco.Length){
				bloco[x] = Dados.Historico[x-1].texto();
				x++;
			}
			string local = "arquivos/historico/"+texto[1].texto+".txt";
			System.IO.File.WriteAllLines(local, bloco);
		}

		GUI.skin.label.fontSize = (int)(y*2);
		texto[0].texto = Dados.Historico.Length.ToString();
		texto[0].desenha();
		texto[1].desenhaField();//campo do nome do arquivo de .txt
		if(texto[1].texto == "")texto[1].texto = "teste";//se for totalmente apagado assume valor padrão "teste"
	}
}

[Serializable ()]
public class GUIHistorico{
	public GUITexto[] palavra;
	public GUIButton editarRect;
	public GUIButton[] bot;
	//construtor padrão
	public GUIHistorico(){
		palavra = new GUITexto[17];
		int x = 0;
		while(x<palavra.Length){
			palavra[x] = new GUITexto();
			x++;
		}
		editarRect = new GUIButton();
	}
	//função que coleta os dados para a linha que será desenhada
	public void pegaDados(historico histExterno){
		historico evento = (historico)histExterno.clone();
		palavra[0].texto = (string)evento.horaEmbarque;
		palavra[2].texto = (string)evento.estOrigem.nome;
		palavra[4].texto = (string)evento.respEmbarque.nome;
		palavra[6].texto = (string)evento.tipo.ToString();
		palavra[8].texto = (string)evento.tremTexto();
		palavra[10].texto = (string)evento.estDestino.nome;
		palavra[12].texto = (string)evento.respDesembarque.nome;
		palavra[14].texto = (string)evento.horaDesembarque;
		palavra[16].texto = (string)evento.observacao;
	}
	//método que escreve a linha na box
	public void desenha(float scrollValor, int y, historicoGUI HistoricoGUI, bool editavel){
		int x = 0;
		while(x<palavra.Length){
			palavra[x].desenha(0, (-scrollValor+y)/14);//método desenha dinâmico recebendo valores relativos a posição e alteração baseada na scrollbar
			x++;
		}
		if(editavel){
			//botão de editar
			if(bot[0].desenha(0, (-scrollValor+y)/14)){
				HistoricoGUI.Dados.rumo = comportamento.EditarHist;
				HistoricoGUI.Dados.eventoDin.recebeHistorico(HistoricoGUI.Dados.Historico[y]);
				HistoricoGUI.FuncGUI.HistEditado = HistoricoGUI.Dados.Historico[y];
				HistoricoGUI.hide = true;
				HistoricoGUI.MapaGUI.hide = false;
				HistoricoGUI.MapaGUI.Origem = true;
			}
			//botão X de excluir historico
			if(bot[1].desenha(0, (-scrollValor+y)/14)){
				historico[] hTemp = HistoricoGUI.Dados.Historico;//armazenando dados do vetor para resize
				HistoricoGUI.Dados.Historico = new historico[hTemp.Length-1];//resize
				int z = 0;
				while(z<HistoricoGUI.Dados.Historico.Length){//resgatando dados do vetor temporario
					HistoricoGUI.Dados.Historico[z]=hTemp[z];
					z++;
				}
				z=y;
				while(z<HistoricoGUI.Dados.Historico.Length){//reposicionando elementos para ocupar a vaga do excluido
					HistoricoGUI.Dados.Historico[z]=hTemp[z+1];
					z++;
				}
				HistoricoGUI.Dados.saveLoadGeral(2, HistoricoGUI.MainGUI.campos[4].texto);//salvando historico
			}
		}
	}

	public object clone(){
		return this.MemberwiseClone();
	}
}