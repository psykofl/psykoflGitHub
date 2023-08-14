//versão 2013.02.05a
using UnityEngine;
using System.Collections;
using System;
using System.Globalization;
using System.Threading;

using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using System.Runtime.Serialization;
using System.Reflection;

public class dados : MonoBehaviour {
	trava Trava;//script que trava o programa, caso este esteja vencido
	//dados de personalização do programa
	public float absolutoPersonalizado;//tempo personalizado que o programa alarma para que o controlador informe o evento
	public bool alarme;//atuação ou não do alarme sonoro
	AudioSource ruido;
		//dados de orientação de comportamento do script
	public eventoD eventoDin;
	public comportamento rumo;
	public usuario Usuario;

	//banco de dados de estações e quadro operativo de funcionários
	public estacao[] Estacao;//BD das estações, salvo em arquivo no HD
	public funcionario[] quadroEmpregados;//BD do quadro de empregados, salvo em arquivo no HD
	public bool ordenado;
	
	//tabela 2D de intervalos de tempo de viajem entre as estações
	public int[,] intervalos;//BD dos intervalos de tempo de viajem entre as estações, salvo em arquivo no HD

	//tabelas de acompanhamentos
	public eventoD[] EventoD;//fila de dados de usuários carentes de acompanhamento
	public historico[] Historico;//relatorio de todos os eventos ocorridos no dia, salvo em arquivo no HD
	//public string diaDeHoje;//string usada para criar o arquivo do historico
	mainGUI MainGUI;

	// Use this for initialization
	void Start () {
		ordenado = false;

		Trava = GetComponent<trava>();

		ruido = GetComponent<AudioSource>();

		rumo = comportamento.novo;
		relocaEventoDin();
		try{
			saveLoadGeral(7);//carregar intervalos
		}catch{
			intervalos = new int[34,34];
		}
		try{
			saveLoadGeral(1);//carregar eventos
		}catch{
			EventoD = new eventoD[0];
		}
		saveLoadGeral(5);//carregar quadro de empregados

		MainGUI = GetComponent<mainGUI>();
	}

	// Update is called once per frame
	void Update () {
		//parte responsável por gerenciar o vetor de eventos e alarme
		int x = 0;
		int y = 0;//inteiro que o acionador do alarme usa
		while(x<EventoD.Length){
			//parte que dispara o alarme
			if(!EventoD[x].silenciado&&EventoD[x].absoluto<absolutoPersonalizado&&!ruido.isPlaying)ruido.Play();
			if(EventoD[x].absoluto>0)EventoD[x].absoluto -=Time.deltaTime;
			//se o absoluto deste evento permanece maior do q absolutoPersonalizado OR estiver silenciado, então some 1 ao y
			if(EventoD[x].absoluto>absolutoPersonalizado||EventoD[x].silenciado)y++;
			if(EventoD[x].absoluto<0)EventoD[x].absoluto = 0;

			//parte que reposiciona elementos com maior prioridade na fila de eventos
				//caso exista um evento na posição x+1
			if((x+1)<EventoD.Length){
				if(((!EventoD[x].ativo)||(EventoD[x].absoluto > EventoD[x+1].absoluto))&&(EventoD[x+1].ativo)){
					//execução de retroceder este evento "x" dentro da fila
					eventoD eventDTemp = EventoD[x];
					EventoD[x] = EventoD[x+1];
					EventoD[x+1] = eventDTemp;
				}
			}
			x++;
		}
			//se y tiver valor igual a quantidade total de eventos, então silencia o alarme
		if (y==EventoD.Length || !alarme)ruido.Stop();
		y = 0;

			//se tiver algum elemento e o ultimo estiver inativo, que seja excluido
		if(EventoD.Length>0&&!EventoD[(EventoD.Length-1)].ativo){
			//aumentando o range do vetor de historico para armazenar o evento inativo antes de sua destruição
			historico[] temp = Historico;
			//x = (int)Historico.Length;
			Historico = new historico[(temp.Length+1)];
			x=0;
			while(x<temp.Length){
				Historico[x] = temp[x];
				x++;
			}
			//designando os valores do evento a ser excluido ao novo elemento do historico	//Historico[(Historico.Length-1)] = new historico();
			Historico[x] = new historico();
			Historico[x].recebeEvento((eventoD)EventoD[(EventoD.Length-1)].clone());

			//excluindo o ultimo elemento do vetor de eventos
			eventoD[] tempE = EventoD;
			//x = (int)EventoD.Length;
			EventoD = new eventoD[tempE.Length-1];
			x=0;
			while(x<EventoD.Length){
				EventoD[x] = tempE[x];
				x++;
			}
			saveLoadGeral(0);//salvar eventos
			saveLoadGeral(2, MainGUI.campos[4].texto);//salvar historico
		}

		//parte que coloca em ordem alfabética e exclui elementos inativos do vetor quadroEmpregados
		if(!ordenado){
			ordenado = true;
			x=0;
			while(x<quadroEmpregados.Length){
				if(x+1<quadroEmpregados.Length){
						//OR o empregado é nulo AND seu sucessor é valido
					if((quadroEmpregados[x].nome==""&&quadroEmpregados[x+1].nome!="") ||
					  //OR na comparação da string o sucessor tenha prioridade alfabeticamente
					  string.Compare(quadroEmpregados[x].nome,quadroEmpregados[x+1].nome)>0){
						funcionario temp = quadroEmpregados[x];
						quadroEmpregados[x] = quadroEmpregados[x+1];
						quadroEmpregados[x+1] = temp;
						ordenado = false;
					}
				}
				x++;
			}
		
				//exclusão do ultimo elemento caso ele seja nulo
			if(quadroEmpregados[quadroEmpregados.Length-1].nome==""){
				funcionario[] qTemp = quadroEmpregados;
				x = (int)quadroEmpregados.Length;
				quadroEmpregados = new funcionario[qTemp.Length-1];
				x=0;
				while(x<quadroEmpregados.Length){
					quadroEmpregados[x] = qTemp[x];
					x++;
				}
			}
		}
	}

	void OnApplicationQuit (){
		saveLoadGeral(0);
	}

	//método que adciona um novo evento ao vetor de eventos
	public void novoEvento(eventoD eventoRec){
		eventoD[] temp = EventoD;
		int x = (int)EventoD.Length;
		EventoD = new eventoD[x+1];
		x=0;
		while(x<temp.Length){
			if(temp.Length>0)EventoD[x] = temp[x];
			x++;
		}
		EventoD[(temp.Length)] = (eventoD)eventoRec.clone();
		relocaEventoDin();
		saveLoadGeral(0);//salvar eventos
	}

	public void relocaEventoDin(){
		eventoDin = new eventoD();
	}

	public void novoFuncionario (funcionario novato){
		funcionario novo = (funcionario)novato.clone();
		funcionario[] temp = quadroEmpregados;
		int x = quadroEmpregados.Length;
		quadroEmpregados = new funcionario[x+1];
		x = 0;
		while(x<temp.Length){
			quadroEmpregados[x] = temp[x];
			x++;
		}
		quadroEmpregados[x] = novo;
	}

	public void saveLoadGeral (int comandoID, string DiaDeHoje){
		string diaD = "arquivos/historico/"+DiaDeHoje+".pca";//diaD é a string diaDeHoje localizada e formatada
		switch (comandoID){
			case 0: //salvar eventos
				SaveLoad.Save("arquivos/eventos.pca", EventoD);
				break;
			case 1: //carregar eventos
				try{
					EventoD = (eventoD[])SaveLoad.Load("arquivos/eventos.pca");
				}catch{
					EventoD = new eventoD[0];
					break;
				}
				break;
			case 2: //salvar historico
				SaveLoad.Save(diaD, Historico);
				break;
			case 3: //carregar historico
				try{
					Historico = (historico[])SaveLoad.Load(diaD);
				}catch{
					Historico = new historico[0];
					break;
				}
				break;
			case 4: //salvar quadro de empregados
				SaveLoad.Save("arquivos/empregados.pca", quadroEmpregados);
				break;
			case 5: //carregar quadro de empregados
				try{
					quadroEmpregados = (funcionario[])SaveLoad.Load("arquivos/empregados.pca");
				}catch{
					break;
				}
				break;
			case 6: //salvar intervalos
				SaveLoad.Save("arquivos/intervalos.pca", intervalos);
				break;
			case 7: //carregar intervalos
				try{
					intervalos = (int[,])SaveLoad.Load("arquivos/intervalos.pca");
				}catch{
					break;
				}
				break;
			default:
				break;
		}
	}

	//overload da saveLoadGeral sem o argumento que atualiza o diaDeHoje
	public void saveLoadGeral(int comandoID){
		saveLoadGeral (comandoID, "");
	}

	//aquisição da data do windows e formatação para obter no formato: "hora:minuto"
	public string horaAgora (){
		string dataWind = dataOriginal();
		string dataCor = null;
		int x = 0;                             //navegador de char do dataWind
		int y = dataWind.Length;//inteiro que marca tamanho da data adiquirida
		y -= 3;                                //parte que ignora os segundos
		bool achamos = false;
		while (x < y){
			if(achamos){
				dataCor += dataWind[x].ToString();
			}
			if(dataWind[x] == ' '){
				achamos = true;
			}
			x++;
		}
		//esta parte tenta enviar a data crua para o script de trava
		try{
			Trava.dia = DateTime.Now.Day.ToString();
			Trava.mes = DateTime.Now.Month.ToString();
			Trava.ano = DateTime.Now.Year.ToString();
			Trava.validade();
		}catch{}
		return dataCor;
	}
	public string dataOriginal(){
		//Thread.CurrentThread.CurrentCulture = new CultureInfo("pt-BR");
		return DateTime.Now.ToString();
	}
}

[Serializable ()]
public class estacao{
	public string nome;
	public int ID;
}

[Serializable ()]
public class funcionario{
	public string mat;
	public string nome;
	public int linkID;

	public funcionario(){
		nome = "";
		mat = "";
	}

	public object clone(){
		return this.MemberwiseClone();
	}
}

[Serializable ()]
public enum deficiencia{DV = 0, CD = 1, DL = 2, OUTR = 3, nulo = 4}

[Serializable ()]
public enum comportamento{novo = 0, Editar = 1, posit = 2, infor = 3, EditarHist = 4}

[Serializable ()]
public enum usuario{operador = 0, option = 1, editor = 2}

[Serializable ()]
public class historico{
	//dados do informante
	public string horaEmbarque;
	public estacao estOrigem;
	public funcionario respEmbarque;
	
	//dados do deficiente
	public deficiencia tipo;
		//dados do trem onde o deficiente embarcou
	public string tremNumero;
	/*public string tremCarro;
	public int tremPorta;*/
	public string tremTexto(){
		return ("Trem "+tremNumero/*+"C"+tremCarro+"P"+tremPorta*/);
	}

	//dados do receptor
	public estacao estDestino;
	public funcionario respDesembarque;
	public string horaDesembarque;

	//observações gerais a respeito desse evento
	public string observacao;

	//método de inicialização padrão deste objeto
	public historico (){
		estOrigem = new estacao();
		respEmbarque = new funcionario();
		tipo = deficiencia.nulo;
		estDestino = new estacao();
		respDesembarque = new funcionario();
		tremNumero = "";
		/*tremCarro = "";
		tremPorta = 0;*/
	}
	
	//método para converter evento em histórico
	public void recebeEvento(eventoD EVento){
		eventoD Evento = (eventoD)EVento.cloneEv();

		horaEmbarque = Evento.horaEmbarque;
		estOrigem = Evento.estOrigem;
		respEmbarque = Evento.respEmbarque;
		tipo = Evento.tipo;
		tremNumero = Evento.tremNumero;
		/*tremCarro = Evento.tremCarro;
		tremPorta = Evento.tremPorta;*/
		estDestino = Evento.estDestino;
		respDesembarque = Evento.respDesembarque;
		horaDesembarque = Evento.horaDesembarque;
		observacao = Evento.observacao;
	}

	//método que retorna e aloca memória para armazenar uma copia deste objeto
	public object clone (){
		return this.MemberwiseClone();
	}
	
	//linha de texto de formulário
	public string texto(){
		return (horaEmbarque+"|"+estOrigem.nome+"|"+respEmbarque.nome+"|"+tipo.ToString()+"|"+tremTexto()+"|"
		        +estDestino.nome+"|"+respDesembarque.nome+"|"+horaDesembarque+"|"+observacao);
	}
}

[Serializable ()]
public class eventoD : historico{
	public bool silenciado;
	public bool ativo;

	public float absoluto;

	//construtor padrão
	public eventoD(){
		ativo = true;
		absoluto = 0;
	}

	//métodos do tempo absoluto que resta para o desembarque
	public void atualiza(int[,] intervalos, estacao origem, estacao destino){
		absoluto = (float)intervalos[origem.ID,destino.ID];
	}
	public string absolutoTexto(){
		int hora = (int)(absoluto/3600);
		int minuto = (int)((absoluto-(hora*3600))/60);
		int segundo = (int)(absoluto-(hora*3600)-(minuto*60));
		if(hora!=0){
			return string.Format("{0:0,#}",hora)+":"+string.Format("{0:0,#}",minuto)+":"+string.Format("{0:0,#}",segundo);
		}else if(minuto!=0){
			return "00:"+string.Format("{0:0,#}",minuto)+":"+string.Format("{0:0,#}",segundo);
		}else{
			return "00:"+"00:"+string.Format("{0:0,#}",segundo);
		}
	}

	//método para converter histórico em evento
	public void recebeHistorico(historico Historico){
		historico HistoricoN = (historico)Historico.clone();

		horaEmbarque = HistoricoN.horaEmbarque;
		estOrigem = HistoricoN.estOrigem;
		respEmbarque = HistoricoN.respEmbarque;
		tipo = HistoricoN.tipo;
		tremNumero = HistoricoN.tremNumero;
		/*tremCarro = HistoricoN.tremCarro;
		tremPorta = HistoricoN.tremPorta;*/
		estDestino = HistoricoN.estDestino;
		respDesembarque = HistoricoN.respDesembarque;
		horaDesembarque = HistoricoN.horaDesembarque;
		observacao = HistoricoN.observacao;
	}

	public object cloneEv(){
		return this.MemberwiseClone();
	}
}

// === This is the class that will be accessed from scripts ===
public class SaveLoad {

	//public static SaveData data = new SaveData (); //instanciando data para usar em save( ) e load( )  

	public static string currentFilePath = "SaveData.cjc";    // Edit this for different save file path and name

	// Call this to write data
	public static void Save (string filePath, object data)
	{
		Stream stream = File.Open(filePath, FileMode.Create);
		BinaryFormatter bformatter = new BinaryFormatter();
		bformatter.Binder = new VersionDeserializationBinder(); 
		bformatter.Serialize(stream, data);
		stream.Close();
	}

	// Call this to load from a file into "data"
	public static object Load (string filePath) 
	{
		object data = new object();
		Stream stream = File.Open(filePath, FileMode.Open);
		BinaryFormatter bformatter = new BinaryFormatter();
		bformatter.Binder = new VersionDeserializationBinder(); 
		data = (object)bformatter.Deserialize(stream);
		stream.Close();
		return data;
		// Now use "data" to access your Values
	}
}

// === This is required to guarantee a fixed serialization assembly name, which Unity likes to randomize on each compile
// Do not change this
public sealed class VersionDeserializationBinder : SerializationBinder 
{ 
    public override Type BindToType( string assemblyName, string typeName )
    { 
        if ( !string.IsNullOrEmpty( assemblyName ) && !string.IsNullOrEmpty( typeName ) ) 
        { 
            Type typeToDeserialize = null; 

            assemblyName = Assembly.GetExecutingAssembly().FullName; 

            // The following line of code returns the type. 
            typeToDeserialize = Type.GetType( String.Format( "{0}, {1}", typeName, assemblyName ) ); 

            return typeToDeserialize; 
        } 

        return null; 
    } 
}

[Serializable()]
class WriteTextFile
{
	static void Main()
	{
		// These examples assume a "C:\Users\Public\TestFolder" folder on your machine.
		// You can modify the path if necessary. 

		// Example #1: Write an array of strings to a file. 
		// Create a string array that consists of three lines. 
		string[] lines = {"First line", "Second line", "Third line"};
		System.IO.File.WriteAllLines(@"C:\Users\Public\TestFolder\WriteLines.txt", lines);


		// Example #2: Write one string to a text file. 
 		string text = "A class is the most powerful data type in C#. Like structures, " +
			"a class defines the data and behavior of the data type. ";
		System.IO.File.WriteAllText(@"C:\Users\Public\TestFolder\WriteText.txt", text);

		// Example #3: Write only some strings in an array to a file. 
		using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\Public\TestFolder\WriteLines2.txt"))
		{
			foreach (string line in lines)
			{
				if (line.Contains("Second") == false)
				{
					file.WriteLine(line);
				}
			}
		}

		// Example #4: Append new text to an existing file 
		using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\Public\TestFolder\WriteLines2.txt", true))
		{
			file.WriteLine("Fourth line");
		}  
	}
}
/* Output (to WriteLines.txt):
	First line
	Second line
	Third line

	Output (to WriteText.txt):
		A class is the most powerful data type in C#. Like structures, a class defines the data and behavior of the data type.

	Output to WriteLines2.txt after Example #3:
		First line
		Third line

	Output to WriteLines2.txt after Example #4:
		First line
		Third line
		Fourth line
*/