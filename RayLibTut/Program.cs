using Raylib_cs;
using System.Numerics;
using System.Collections.Generic;
using System;
using Slicer3D.Domain;
using Slicer3d;
using Slicer3D;

namespace HelloWorld
{
	static class Program
	{
		public static void Main()
		{
			// Exemplo de configuração de camadas
			Layers slicingSettings = new Layers(2, 4, 4, 2, OutlineDirection.InsideOut);

			string filePath = "G:\\Meu Drive\\Codigos\\Slicer3D\\Slicer3D\\teste\\teste.txt";
			List<Triangle> mesh = ReadTriangles(filePath);
			Console.WriteLine($"{mesh.Count} triangulos lidos");

			// Cria uma instância da classe Fatiador
			Fatiador fatiador = new Fatiador();
			// Executa o fatiamento e obtém as interseções entre os vértices de cada triângulo e cada altura gerada
			var perimetros = fatiador.ExecutarFatiamento(slicingSettings.LayerHeight, mesh);



			// Cria uma instância da classe ConversorSTL
			ConversorSTL conversorSTL = new ConversorSTL();
			List<Vector3> espherePositionsList = new List<Vector3>();

			foreach (var listListperimetro in perimetros)
			{
				foreach (var listperimetro in listListperimetro)
				{
					foreach (var perimetro in listperimetro)
					{
						Vector3 espherePositions = new Vector3(perimetro.X * 0.1f, perimetro.Y * 0.1f, perimetro.Z * 0.1f);
						espherePositionsList.Add(espherePositions);
						
					}
				}
			}




			Raylib.InitWindow(1900, 1200, "Hello World");
			Raylib.SetTargetFPS(60);
			Raylib.GetFPS();

			Camera3D camera = new Camera3D();
			camera.position = new Vector3(0.0f, 10.0f, 10.0f);  // Camera position
			camera.target = new Vector3(0.0f, 0.0f, 0.0f);      // Camera looking at point
			camera.up = new Vector3(0.0f, 1.0f, 0.0f);          // Camera up vector (rotation towards target)
			camera.fovy = 45.0f;                                // Camera field-of-view Y
			camera.projection = CameraProjection.CAMERA_PERSPECTIVE;  // Camera mode type

			// Set the number of cubes
			int numCubes = 10;

			// Create a list to store the cube positions
			List<Vector3> cubePositions = new List<Vector3>();

			// Create a Random instance
			Random rnd = new Random();

			// Initialize cube positions with random values
			for (int i = 0; i < numCubes; i++)
			{
				float posX = (float)rnd.NextDouble() * 10.0f - 5.0f;  // Random X position between -5 and 5
				float posY = (float)rnd.NextDouble() * 10.0f - 5.0f;  // Random Y position between -5 and 5
				float posZ = (float)rnd.NextDouble() * 10.0f - 5.0f;  // Random Z position between -5 and 5

				cubePositions.Add(new Vector3(posX, posY, posZ));
			}

			while (!Raylib.WindowShouldClose())
			{
				Raylib.UpdateCamera(ref camera, CameraMode.CAMERA_THIRD_PERSON);
				
				Raylib.BeginDrawing();

				Raylib.BeginMode3D(camera);
				Raylib.ClearBackground(Color.WHITE);

				// Draw each cube in the cubePositions list
				foreach (Vector3 cubePosition in cubePositions)
				{
					//Raylib.DrawCube(cubePosition, 1.0f, 1.0f, 1.0f, Color.RED);
				}
				foreach (Vector3 espherePositions in espherePositionsList)
				{
					Raylib.DrawSphere(espherePositions, 0.1f, Color.RED);
					
				}
				var segundoPonto = new Vector3(0f, 0f, 0f);
				foreach (Vector3 espherePositions in espherePositionsList)
				{
					var primeiroPonto = espherePositions;
					Raylib.DrawLine3D(primeiroPonto, segundoPonto, Color.BLACK);
					 segundoPonto = primeiroPonto;
				}


				Raylib.DrawGrid(10, 1.0f);
				Raylib.EndMode3D();
				Raylib.DrawText("Hello, world!", 12, 12, 20, Color.BLACK);
				Raylib.EndDrawing();
			}

			Raylib.CloseWindow();
		}


		public static List<Triangle> ReadTriangles(string filePath)
		{
			StreamReader sr = new StreamReader(filePath);
			string data = sr.ReadToEnd();
			string[] result = data.Split('\n');
			bool inLoop = false;
			int n = 0;

			List<Triangle> mesh = new List<Triangle>();

			for (int i = 0; i < (result.Length); i++)
			{
				var ler = result[i];
				int NPoint = 0;
				if (result[i] == "  outer loop\r")
				{
					//Console.WriteLine("  outer loop");
					inLoop = true;
					string[] vertex1 = result[i + 1].Split(' ').Skip(5).ToArray();
					string[] vertex2 = result[i + 2].Split(' ').Skip(5).ToArray();
					string[] vertex3 = result[i + 3].Split(' ').Skip(5).ToArray();

					Triangle newTriangle = new Triangle(n);
					n++;
					mesh.Add(newTriangle);

					newTriangle.Poin1.X = float.Parse(vertex1[0]);
					newTriangle.Poin1.Y = float.Parse(vertex1[1]);
					newTriangle.Poin1.Z = float.Parse(vertex1[2]);

					newTriangle.Poin2.X = float.Parse(vertex2[0]);
					newTriangle.Poin2.Y = float.Parse(vertex2[1]);
					newTriangle.Poin2.Z = float.Parse(vertex2[2]);

					newTriangle.Poin3.X = float.Parse(vertex3[0]);
					newTriangle.Poin3.Y = float.Parse(vertex3[1]);
					newTriangle.Poin3.Z = float.Parse(vertex3[2]);

					i = i + 4;
				}

				if (result[i] == "endloop")
					inLoop = false;

				if (inLoop == true)
				{
					NPoint++;
				}
			}
			sr.Close();

			return mesh;
		}
		public static void ImprimirPontos(List<List<List<IntersectingPoint>>> perimetros)
		{
			int alturaIndex = 1;
			foreach (var altura in perimetros)
			{
				Console.WriteLine($"Altura {alturaIndex}:");
				int perimetroIndex = 1;
				foreach (var perimetro in altura)
				{
					Console.WriteLine($"\tPerimetro {perimetroIndex}:");
					int pontoIndex = 1;
					foreach (var ponto in perimetro)
					{
						Console.WriteLine($"\t\tPonto {pontoIndex}: ({ponto.X}, {ponto.Y}, {ponto.Z}, de id: {ponto.TriangleId})");
						pontoIndex++;
					}
					perimetroIndex++;
				}
				alturaIndex++;
			}
		}

	}
}
