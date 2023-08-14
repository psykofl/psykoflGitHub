using UnityEngine;
using System.Collections;

public class mapaGUI : MonoBehaviour {
	dados Dados;
	public GUISkin GSkin;
	float y = 0;//tamanho relativo da font
	mainGUI MainGUI;
	funcGUI FuncGUI;
	tipoTremGUI TipoTremGUI;
	historicoGUI HistoricoGUI;

	public bool hide;
	public bool Origem;

	//rects
	public GUIButton[] bot;
	public GUITexto[] texto;

	void Start () {
		Dados = GetComponent<dados>();
		MainGUI = GetComponent<mainGUI>();
		FuncGUI = GetComponent<funcGUI>();
		TipoTremGUI = GetComponent<tipoTremGUI>();
		HistoricoGUI = GetComponent<historicoGUI>();
	}

	void OnGUI (){
		if(hide)return;
		GUI.skin = GSkin;
		if(y != Screen.height*0.02f){
			y = Screen.height*0.02f;
		}
		GUI.skin.button.fontSize = (int)(y*1.8f);
		GUI.skin.label.fontSize = (int)(y*1.5f);

		//GUI.Box(mainBox.ajuste(),"");      //box gráfica
		//botão de voltar
		if(bot[0].desenha()){
			hide = true;
			if(Dados.rumo != comportamento.posit){
				if(Origem == true){
					if(Dados.rumo == comportamento.EditarHist){
						HistoricoGUI.hide = false;
					}else{
						MainGUI.hide = false;
					}
					Dados.relocaEventoDin();
				}else{
					TipoTremGUI.hide = false;
				}
			}else{
				MainGUI.hide = false;
			}
		}
		//botões das estações
		GUI.skin.button.fontSize = (int)(y*0.8f);
		int x = 1;
		while(x<bot.Length&&x<34){
			if(bot[x].desenha()){
				hide = true;
				//se esta tela foi chamada pelo posit de algum evento
				if(Dados.rumo == comportamento.posit){
					Dados.eventoDin.atualiza(Dados.intervalos, Dados.Estacao[x], Dados.eventoDin.estDestino);
					Dados.relocaEventoDin();
					MainGUI.hide = false;
				//se esta tela não for posit mas estiver sido acionada para informar a origem
				}else if(Origem==true){
					Dados.eventoDin.estOrigem = Dados.Estacao[x];
					FuncGUI.hide = false;
					FuncGUI.origem = true;
					FuncGUI.text[5].texto = "";
					FuncGUI.text[6].texto = "";
				//se esta tela não for posit e estiver sido acionada para informar o destino
				}else{
					Dados.eventoDin.estDestino = Dados.Estacao[x];
					if(Dados.rumo == comportamento.novo){
						MainGUI.hide = false;
						Dados.eventoDin.atualiza(Dados.intervalos, Dados.eventoDin.estOrigem, Dados.eventoDin.estDestino);
						Dados.novoEvento(Dados.eventoDin);
					}else if(Dados.rumo == comportamento.Editar || Dados.rumo == comportamento.EditarHist){
						FuncGUI.hide = false;
						FuncGUI.origem = false;
					}
				}
				FuncGUI.text[4].texto = "";
			}
			x++;
		}
		//botão "Avançar"
		GUI.skin.button.fontSize = (int)(y*1.8f);
		if(Dados.rumo == comportamento.Editar || Dados.rumo == comportamento.EditarHist){
			if(bot[x].desenha()){
				hide = true;
				FuncGUI.hide = false;
				if(Origem){
					FuncGUI.origem = true;
				}else{
					FuncGUI.origem = false;
				}
			}
			FuncGUI.text[4].texto = "";
		}

		if(Origem){
			texto[0].texto = "Origem";
			texto[1].texto = Dados.eventoDin.estOrigem.nome;
		}else{
			texto[0].texto = "Destino";
			texto[1].texto = Dados.eventoDin.estDestino.nome;
		}
		texto[0].desenha();
		texto[1].desenha();
	}
}
