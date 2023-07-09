using System;
using System.Collections.Generic;
using System.IO;
using Slicer3D;
using Slicer3D.Domain;

namespace Slicer3d
{
	public class ConversorSTL
	{
		public void GerarArquivoSTL(List<List<Points>> intersecoesReordenadas, string caminhoArquivo)
		{
			using (StreamWriter sw = new StreamWriter(caminhoArquivo))
			{
				// Escreve o cabeçalho do arquivo STL
				sw.WriteLine("solid slicer3d");

				// Processa as interseções reordenadas para gerar os triângulos no formato STL
				for (int i = 0; i < intersecoesReordenadas.Count - 1; i++)
				{
					List<Points> camadaAtual = intersecoesReordenadas[i];
					List<Points> camadaProxima = intersecoesReordenadas[i + 1];

					int tamanhoMinimo = Math.Min(camadaAtual.Count, camadaProxima.Count);

					if (tamanhoMinimo >= 2)
					{
						for (int j = 0; j < tamanhoMinimo - 1; j++)
						{
							// Primeiro triângulo
							EscreverTrianguloSTL(sw, camadaAtual[j], camadaAtual[j + 1], camadaProxima[j]);

							// Segundo triângulo
							EscreverTrianguloSTL(sw, camadaAtual[j + 1], camadaProxima[j + 1], camadaProxima[j]);
						}
					}
				}

				// Escreve o rodapé do arquivo STL
				sw.WriteLine("endsolid slicer3d");
			}
		}

		private void EscreverTrianguloSTL(StreamWriter sw, Points p1, Points p2, Points p3)
		{
			sw.WriteLine("facet normal 0 0 0"); // A normal pode ser calculada corretamente, caso necessário
			sw.WriteLine("  outer loop");
			sw.WriteLine($"    vertex {p1.X} {p1.Y} {p1.Z}");
			sw.WriteLine($"    vertex {p2.X} {p2.Y} {p2.Z}");
			sw.WriteLine($"    vertex {p3.X} {p3.Y} {p3.Z}");
			sw.WriteLine("  endloop");
			sw.WriteLine("endfacet");
		}
	}
}
