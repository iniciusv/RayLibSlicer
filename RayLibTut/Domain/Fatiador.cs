using System;
using System.Collections.Generic;
using Slicer3D;
using Slicer3D.Domain;

namespace Slicer3d
{
	public class Fatiador
	{
		public List<List<List<IntersectingPoint>>> ExecutarFatiamento(float layerHeight, List<Triangle> mesh)
		{
			List<float> alturas = GerarAlturas(layerHeight, mesh);
			List<List<List<IntersectingPoint>>> result = new List<List<List<IntersectingPoint>>>();

			foreach (float altura in alturas)
			{
				var T = FiltrarTriangulosInterceptados(altura, mesh);
				List<IntersectingPoint> pontosIntersecao = CalcularIntersecoesPlanoHorizontal(altura, mesh);
				//var onlyPointsInPerimeter = RemoverIdsRepetidosTresVezes(pontosIntersecao);
				var perimetros = GerarPerimetros(pontosIntersecao);
				List<List<IntersectingPoint>> eliminarPontosRepetidosSeguidos = EliminarPontosRepetidosSeguidos(perimetros);

				result.Add(perimetros);
			}

			return result;
		}
		public List<float> GerarAlturas(float layerHeight, List<Triangle> mesh)
		{
			float maxAltura = 0.0f;

			// Encontra a altura máxima na lista de triângulos (mesh)
			foreach (Triangle triangle in mesh)
			{
				maxAltura = Math.Max(maxAltura, triangle.Poin1.Z);
				maxAltura = Math.Max(maxAltura, triangle.Poin2.Z);
				maxAltura = Math.Max(maxAltura, triangle.Poin3.Z);
			}

			// Cria uma lista de alturas com base na altura máxima e na altura da camada (LayerHeight)
			List<float> alturas = new List<float>();
			for (float altura = 0; altura <= maxAltura; altura += layerHeight)
			{
				alturas.Add(altura);
			}

			return alturas;
		}


		public List<List<IntersectingPoint>> EliminarPontosRepetidosSeguidos(List<List<IntersectingPoint>> intersecoes)
		{
			List<List<IntersectingPoint>> intersecoesSemDuplicatas = new List<List<IntersectingPoint>>();

			foreach (List<IntersectingPoint> intersecoesAltura in intersecoes)
			{
				List<IntersectingPoint> intersecoesAlturaSemDuplicatas = new List<IntersectingPoint>();

				for (int i = 0; i < intersecoesAltura.Count; i++)
				{
					// Adiciona o primeiro ponto e compara os pontos subsequentes com o anterior
					if (i == 0 || !PontosIguais(intersecoesAltura[i], intersecoesAltura[i - 1]))
					{
						intersecoesAlturaSemDuplicatas.Add(intersecoesAltura[i]);
					}
				}

				intersecoesSemDuplicatas.Add(intersecoesAlturaSemDuplicatas);
			}

			return intersecoesSemDuplicatas;
		}

		private bool PontosIguais(Points p1, Points p2)
		{
			return Math.Abs(p1.X - p2.X) < 1e-6f && Math.Abs(p1.Y - p2.Y) < 1e-6f && Math.Abs(p1.Z - p2.Z) < 1e-6f;
		}

		public List<List<Points>> ReordenarIntersecoes(List<List<Points>> intersecoes)
		{
			List<List<Points>> intersecoesReordenadas = new List<List<Points>>();
			Points ultimoPonto = null;

			foreach (List<Points> intersecoesAltura in intersecoes)
			{
				Queue<Points> filaIntersecoes = new Queue<Points>(intersecoesAltura);
				List<Points> intersecoesReordenadasAltura = new List<Points>();

				if (ultimoPonto != null)
				{
					Points pontoMaisProximo = EncontrarPontoMaisProximo(ultimoPonto, filaIntersecoes);
					filaIntersecoes = ReordenarFila(filaIntersecoes, pontoMaisProximo);
				}

				while (filaIntersecoes.Count > 0)
				{
					Points pontoAtual = filaIntersecoes.Dequeue();
					intersecoesReordenadasAltura.Add(pontoAtual);
					ultimoPonto = pontoAtual;
				}

				intersecoesReordenadas.Add(intersecoesReordenadasAltura);
			}

			return intersecoesReordenadas;
		}

		private Points EncontrarPontoMaisProximo(Points referencia, Queue<Points> pontos)
		{
			Points pontoMaisProximo = null;
			double menorDistancia = double.MaxValue;

			foreach (Points ponto in pontos)
			{
				double distancia = CalcularDistanciaXY(referencia, ponto);

				if (distancia < menorDistancia)
				{
					menorDistancia = distancia;
					pontoMaisProximo = ponto;
				}
			}

			return pontoMaisProximo;
		}

		private Queue<Points> ReordenarFila(Queue<Points> fila, Points pontoInicial)
		{
			Queue<Points> filaReordenada = new Queue<Points>();
			List<Points> lista = new List<Points>(fila);
			lista.Remove(pontoInicial);
			filaReordenada.Enqueue(pontoInicial);

			while (lista.Count > 0)
			{
				Points pontoAtual = lista[0];
				lista.RemoveAt(0);
				filaReordenada.Enqueue(pontoAtual);
			}

			return filaReordenada;
		}

		private double CalcularDistanciaXY(Points p1, Points p2)
		{
			return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
		}
		public List<List<Points>> EliminarPontosRepetidos(List<List<Points>> intersecoes)
		{
			List<List<Points>> intersecoesSemRepetidos = new List<List<Points>>();

			foreach (List<Points> camada in intersecoes)
			{
				List<Points> camadaSemRepetidos = new List<Points>();
				foreach (Points ponto in camada)
				{
					bool pontoRepetido = false;
					foreach (Points pontoExistente in camadaSemRepetidos)
					{
						if (ponto.X == pontoExistente.X && ponto.Y == pontoExistente.Y && ponto.Z == pontoExistente.Z)
						{
							pontoRepetido = true;
							break;
						}
					}

					if (!pontoRepetido)
					{
						camadaSemRepetidos.Add(ponto);
					}
				}

				intersecoesSemRepetidos.Add(camadaSemRepetidos);
			}

			return intersecoesSemRepetidos;
		}

		//-----------------------------------------------------

		public List<int> FiltrarTriangulosInterceptados(float a, List<Triangle> mesh)
		{
			List<int> idsTriangulosInterceptados = new List<int>();

			foreach (Triangle triangle in mesh)
			{
				bool interceptado = false;
				// Verifica se algum vértice do triângulo está no plano horizontal de altura 'a'
				if (Math.Abs(triangle.Poin1.Z - a) < 1e-6 ||
					Math.Abs(triangle.Poin2.Z - a) < 1e-6 ||
					Math.Abs(triangle.Poin3.Z - a) < 1e-6)
				{
					interceptado = true;
				}
				else
				{
					// Verifica se alguma aresta do triângulo é interceptada pelo plano horizontal de altura 'a'
					if ((triangle.Poin1.Z - a) * (triangle.Poin2.Z - a) < 0 ||
						(triangle.Poin2.Z - a) * (triangle.Poin3.Z - a) < 0 ||
						(triangle.Poin3.Z - a) * (triangle.Poin1.Z - a) < 0)
					{
						interceptado = true;
					}
				}

				//Remove triangulos inseridos no plano (verificar essa bosta ta sendo removida diversas vezes)
				if (triangle.Poin1.Z == triangle.Poin2.Z && triangle.Poin1.Z == triangle.Poin3.Z)
					interceptado = false;


				if (interceptado)
				{
					idsTriangulosInterceptados.Add(triangle.Id);
				}
			}

			return idsTriangulosInterceptados;
		}

		public List<IntersectingPoint> CalcularIntersecoesPlanoHorizontal(float a, List<Triangle> mesh)
		{
			List<IntersectingPoint> pontosIntersecao = new List<IntersectingPoint>();
			HashSet<int> verticesAdicionados = new HashSet<int>();

			foreach (Triangle triangle in mesh)
			{

				//if(triangle.Poin1.Z != triangle.Poin2.Z && triangle.Poin2.Z != triangle.Poin3.Z)
				//{

					for (int i = 0; i < 3; i++)
					{
						Points p1 = i == 0 ? triangle.Poin1 : (i == 1 ? triangle.Poin2 : triangle.Poin3);
						Points p2 = i == 0 ? triangle.Poin2 : (i == 1 ? triangle.Poin3 : triangle.Poin1);

							if (Math.Abs(p1.Z - a) < float.Epsilon && !verticesAdicionados.Contains(p1.GetHashCode()))
							{
								pontosIntersecao.Add(new IntersectingPoint(p1.X, p1.Y, p1.Z, triangle.Id));
								verticesAdicionados.Add(p1.GetHashCode());
							}

							if (Math.Abs(p2.Z - a) < float.Epsilon && !verticesAdicionados.Contains(p2.GetHashCode()))
							{
								pontosIntersecao.Add(new IntersectingPoint(p2.X, p2.Y, p2.Z, triangle.Id));
								verticesAdicionados.Add(p2.GetHashCode());
							}

						if ((p1.Z - a) * (p2.Z - a) < 0)
						{
							float t = (a - p1.Z) / (p2.Z - p1.Z);
							float x = p1.X + t * (p2.X - p1.X);
							float y = p1.Y + t * (p2.Y - p1.Y);

							pontosIntersecao.Add(new IntersectingPoint(x, y, a, triangle.Id));
						}
					}
				//}
			}

			return pontosIntersecao;
		}


		public List<List<IntersectingPoint>> GerarPerimetros(List<IntersectingPoint> pontos)
		{
			List<List<IntersectingPoint>> perimetros = new List<List<IntersectingPoint>>();

			while (pontos.Count > 0)
			{
				List<IntersectingPoint> perimetro = new List<IntersectingPoint>();
				IntersectingPoint pontoAtual = pontos[0];
				pontos.RemoveAt(0);
				perimetro.Add(pontoAtual);

				while (true)
				{
					IntersectingPoint proximoPonto = null;
					List<IntersectingPoint> candidatosErrados = new List<IntersectingPoint>();

					foreach (IntersectingPoint candidato in pontos)
					{

						bool mesmoXY = candidato.X == pontoAtual.X && candidato.Y == pontoAtual.Y;
						bool idDiferente = candidato.TriangleId != pontoAtual.TriangleId;
						bool idIgual = candidato.TriangleId == pontoAtual.TriangleId;

						if (idIgual)
						{
							proximoPonto = candidato;
							break;
						}
						if (mesmoXY && idDiferente)
						{
							IntersectingPoint subCandidatoCerto = candidato;
							foreach (IntersectingPoint subCandidato in pontos)
							{
								if (candidato.TriangleId == subCandidato.TriangleId)
								{
									subCandidatoCerto = subCandidato;
								}
								//armazenar candidatos que não são os certos
								candidatosErrados.Add(subCandidato);
							}
								proximoPonto = subCandidatoCerto;
							break;
						}
					}

					if (proximoPonto != null)
					{
						perimetro.Add(proximoPonto);
						pontos.Remove(proximoPonto);
						pontos.RemoveAll(item => candidatosErrados.Contains(item));
						pontoAtual = proximoPonto;
					}
					else
					{
						break;
					}
				}

				perimetros.Add(perimetro);
			}

			return perimetros;
		}




		public List<IntersectingPoint> RemoverIdsRepetidosTresVezes(List<IntersectingPoint> pontosIntersecao)
		{
			Dictionary<int, int> idFrequencia = new Dictionary<int, int>();

			// Conta a frequência de cada ID na lista de pontos de interseção
			for (int i = 0; i < pontosIntersecao.Count; i++)
			{
				int id = pontosIntersecao[i].TriangleId;
				if (idFrequencia.ContainsKey(id))
				{
					idFrequencia[id]++;
				}
				else
				{
					idFrequencia[id] = 1;
				}
			}

			pontosIntersecao.RemoveAll(p => idFrequencia.ContainsKey(p.TriangleId) && idFrequencia[p.TriangleId] == 3);


			return pontosIntersecao;
		}



	}
}
