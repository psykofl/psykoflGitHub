using UnityEngine;
using System.Collections;
using System;

public class funcGUI : MonoBehaviour {
	public dados Dados;
	public funcionario[] quadroFuncTemp;//vetor de funcionarios que armazena temporariamente os empregados filtrados
	public int qtdChar;//inteiro que armazena a quantidade anterior de char da string text[4].texto
	/*objeto que na ultima tela converte o eventoDin para historico, em outro script,
	este objeto recebe o endereço do historico que está sendo editado.*/
	public historico HistEditado;
	public GUISkin GSkin;
	float w = 0;//tamanho relativo da font
	public mainGUI MainGUI;
	mapaGUI MapaGUI;
	public tipoTremGUI TipoTremGUI;
	public historicoGUI HistoricoGUI;

	//rects
	public GUIRect box;//box de fundo do quadro de empregados
	public GUIButton[] bot;
	public GUITexto[] text;
	public GUIRect grupo;
	public GUIScroll scrollVert;

	public GUIFuncionario[] GUIQuadro;
	bool editavel;
	public GUIFuncionario example;
	public funcionario novato;
	public bool editando;

	public bool hide;
	public bool origem;

	void Start () {
		Dados = gameObject.GetComponent<dados>();
		MainGUI = GetComponent<mainGUI>();
		MapaGUI = GetComponent<mapaGUI>();
		TipoTremGUI = GetComponent<tipoTremGUI>();
		HistoricoGUI = GetComponent<historicoGUI>();

		editavel = false;
		origem = true;
		relocNovato();
		editando = false;

		quadroFuncTemp = Dados.quadroEmpregados;
		qtdChar++;
	}

	void OnGUI(){
		if(hide)return;
		GUI.skin = GSkin;
		if(w!=Screen.height*0.02f)w = Screen.height*0.02f;
		GUI.skin.label.fontSize = (int)(w*1.2);
		GUI.skin.textField.fontSize = (int)(w*1.5);
		GUI.skin.button.fontSize = (int)(w*2.2);

		//botão de voltar
		if(bot[0].desenha()){
			hide = true;
			if(Dados.rumo == comportamento.infor){//Dados.rumo == comportamento.novo || Dados.rumo == comportamento.Editar
				MainGUI.hide = false;
			}else{//if(Dados.rumo == comportamento.infor)
				MapaGUI.hide = false;
				MapaGUI.Origem = origem;
			}
		}
		//botão de salvar
		if(bot[1].desenha()){
			novato.nome = text[5].texto;
			novato.mat = text[6].texto;
			if(editando){
				relocNovato();
				editando = false;
			}else Dados.novoFuncionario(novato);
			Dados.ordenado = false;
			Dados.saveLoadGeral(4);//salvar quadro de empregados
			resetaEditando(GUIQuadro);
			text[5].texto = "";
			text[6].texto = "";
		}
		//botão de editar
		if(bot[2].desenha()){
			editavel = !editavel;
		}
		//botão "avançar"
		if(Dados.rumo == comportamento.Editar || Dados.rumo == comportamento.EditarHist){
			if(bot[3].desenha()){
				hide = true;
				if(origem){
					TipoTremGUI.hide = false;
					TipoTremGUI.text[4].texto = Dados.eventoDin.tremNumero.ToString();
					TipoTremGUI.text[5].texto = Dados.eventoDin.observacao;
				}else{
					if(Dados.rumo == comportamento.EditarHist){
						HistEditado.recebeEvento(Dados.eventoDin);
						HistoricoGUI.hide = false;
						Dados.saveLoadGeral(2, MainGUI.campos[4].texto);//salvar historico
					}else{
						MainGUI.hide = false;
						Dados.saveLoadGeral(0);//salvar eventos
					}
					Dados.relocaEventoDin();
				}
			}
		}

		int x=0;
		//textos
		while(x<4){
			text[x].desenha();
			x++;
		}
		//campos
		while(x<7){
			text[x].desenhaField();
			x++;
		}
		//texto 

		//BD do quadro de empregados
		GUI.Box(box.ajuste(),"");//box de fundo do quadro de empregados
		GUI.BeginGroup(grupo.ajuste());
		GUI.skin.button.fontSize = (int)(w*1.6);

		//parte que esconde ou mostra a barra de rolagem lateral para quando houver muitos itens
		if(scrollVert.bottonValor>scrollVert.visibilidade){
			scrollVert.desenhaVertical();
		}

		//parte condicionada a alteração da quantidade de char do procurador
		if(qtdChar!=text[4].texto.Length||!Dados.ordenado){
			quadroFuncTemp = new funcionario[0];
			//parte que filtra os empregados com base no que foi colocado no campo text[4].texto
			x=0;
			int z=0;//navegador de char do campo text[4].texto, iniciais do nome desejado
			bool util = true;
			//parte que filtra e coleta os funcionários que atendem ao requisito
			while(x<Dados.quadroEmpregados.Length){
				z=0;
				util = true;
				while(z<text[4].texto.Length){
					try{
						if(Dados.quadroEmpregados[x].nome[z]!=text[4].texto[z])util=false;
					}catch{
						util=false;
					}
					z++;
				}
				if(util){//se o empregado passar pelo filtro ele será util, o quadroFuncTemp aumenta e recebe este
					funcionario[] tempQD = quadroFuncTemp;
					quadroFuncTemp = new funcionario[tempQD.Length+1];
					z=0;
					while(z<tempQD.Length){
						quadroFuncTemp[z] = tempQD[z];
						z++;
					}
					quadroFuncTemp[z] = Dados.quadroEmpregados[x];
					quadroFuncTemp[z].linkID = x;
				}
				x++;//incremento de x para verificar o proximo empregado do quadro original
			}
			x = 0;
			//parte que equaliza a quantidade de botões com a quantidade de empregados do quadro filtrado
			GUIQuadro = new GUIFuncionario[quadroFuncTemp.Length];
			while(x<GUIQuadro.Length){//pegando os valores do GUIFuncionario padrão
				GUIQuadro[x] = (GUIFuncionario)example.clone();
				x++;
			}
			scrollVert.bottonValor = GUIQuadro.Length;//resize do scrollVert
			scrollVert.valor = 0;//zerando a posição do scrollVert
			qtdChar = text[4].texto.Length;
		}//final da condicional de alteração do campo text[4].texto -> campo de filtro de empregados

		//parte que escreve os botões dos empregados
			//parte que navega em todo o vetor para coletar e desenhar linha a linha
		x = 0;
		while(x<GUIQuadro.Length){
			GUIQuadro[x].pegaDados(quadroFuncTemp[x]);
			GUIQuadro[x].desenha(scrollVert.valor, x, this, editavel);
			x++;
		}

		//Parte para teste e edição da linha padrão "example"
		//example.desenha(scrollVert.valor, 0);
		
		GUI.EndGroup();
	}

	public void relocNovato(){
		novato = new funcionario();
	}

	//função que torna falso todos os "editando" do vetor de obj desta classe
	public void resetaEditando (GUIFuncionario[] GUIQuadro){
		int x = 0;
		while(x<GUIQuadro.Length){
			GUIQuadro[x].editando = false;
			x++;
		}
	}
}

[Serializable ()]
public class GUIFuncionario{
	public bool editando;
	public int IDVetor;

	public GUIButton[] bot;

	//construtor padrão
	public GUIFuncionario(){
		//editando = false;
		IDVetor = 0;

		bot = new GUIButton[3];
		int x = 0;
		while(x<bot.Length){
			bot[x] = new GUIButton();
			x++;
		}
	}
	//função que coleta os dados para a linha que será desenhada
	public void pegaDados(funcionario Funcionario){
		bot[0].conteudo.text = (string)Funcionario.nome+"|"+(string)Funcionario.mat;
		
	}
	//método que escreve a linha na box
	public void desenha(float scrollValor, int y, funcGUI FuncGUI, bool editavel){
		//botão do funcionario
		if(bot[0].desenha(0, (-scrollValor+y)/20)){
			FuncGUI.hide = true;
			if(FuncGUI.Dados.rumo == comportamento.novo){
				FuncGUI.TipoTremGUI.text[4].texto = "";
				FuncGUI.TipoTremGUI.text[5].texto = "CARRO LIDER.";
			}
			if(FuncGUI.origem){
				FuncGUI.Dados.eventoDin.respEmbarque = FuncGUI.quadroFuncTemp[y];
				FuncGUI.TipoTremGUI.hide = false;
			}else{
				FuncGUI.Dados.eventoDin.respDesembarque = FuncGUI.quadroFuncTemp[y];
				if(FuncGUI.Dados.rumo == comportamento.EditarHist){
					FuncGUI.HistEditado.recebeEvento(FuncGUI.Dados.eventoDin);
					FuncGUI.HistoricoGUI.hide = false;
					FuncGUI.Dados.saveLoadGeral(2, FuncGUI.MainGUI.campos[4].texto);//salvar historico
				}else FuncGUI.MainGUI.hide = false;
				if(FuncGUI.Dados.rumo == comportamento.infor){
					FuncGUI.Dados.eventoDin.horaDesembarque = FuncGUI.Dados.horaAgora();
					FuncGUI.Dados.saveLoadGeral(0);//salvar eventos
				}
				FuncGUI.Dados.relocaEventoDin();
			}
			FuncGUI.resetaEditando(FuncGUI.GUIQuadro);
		}
		if(editavel){
			if(editando){
				bot[1].conteudo.text = "Cancelar";
				//o acontecerá ao clicar em Cancelar
				if(bot[1].desenha(0, (-scrollValor+y)/20)){
					FuncGUI.text[5].texto = "";
					FuncGUI.text[6].texto = "";
					editando = false;
					FuncGUI.editando = false;
					FuncGUI.novato = new funcionario();
				}
				//o que acontecerá ao clicar em excluir
				if(bot[2].desenha(0, (-scrollValor+y)/20)){
					FuncGUI.text[5].texto = "";
					FuncGUI.text[6].texto = "";
					FuncGUI.editando = false;
					FuncGUI.Dados.quadroEmpregados[FuncGUI.quadroFuncTemp[y].linkID] = new funcionario();
					FuncGUI.Dados.ordenado = false;
					FuncGUI.Dados.saveLoadGeral(4);//salvar quadro de empregados
				}
			}else{
				bot[1].conteudo.text = "Editar";
				//o acontecerá ao clicar em editar
				if(bot[1].desenha(0, (-scrollValor+y)/20)){
					FuncGUI.text[5].texto = FuncGUI.Dados.quadroEmpregados[FuncGUI.quadroFuncTemp[y].linkID].nome;
					FuncGUI.text[6].texto = FuncGUI.Dados.quadroEmpregados[FuncGUI.quadroFuncTemp[y].linkID].mat;
					FuncGUI.novato = FuncGUI.Dados.quadroEmpregados[FuncGUI.quadroFuncTemp[y].linkID];
					FuncGUI.resetaEditando(FuncGUI.GUIQuadro);
					editando = true;
					FuncGUI.editando = true;
				}
			}
		}
	}

	public object clone(){
		return this.MemberwiseClone();
	}
}