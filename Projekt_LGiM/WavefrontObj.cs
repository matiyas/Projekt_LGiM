﻿using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;
using System.IO;
using System.Globalization;
using System.Windows;

namespace Projekt_LGiM
{
    class WavefrontObj
    {
        public struct Sciana
        {
            public List<int> Vertex { get; set; }
            public List<int> VertexTexture { get; set; }
            public List<int> VertexNormal { get; set; }
        }

        public DenseVector Pozycja { get; set; }
        public DenseVector Obrot { get; set; }
        public DenseVector Skalowanie { get; set; }
        public List<DenseVector> VertexCoords { get; private set; }
        public List<DenseVector> VertexTextureCoords { get; }
        public List<DenseVector> VertexNormalsCoords { get; }
        public List<Sciana> Sciany { get; }
        public List<Sciana> ScianyTrojkatne { get; }
        public Teksturowanie Teksturowanie { get; set; }

        private string sciezka;

        public WavefrontObj(string sciezka)
        {
            this.sciezka = sciezka;

            Pozycja = new DenseVector(3);
            Obrot = new DenseVector(3);
            Skalowanie = new DenseVector(new double[] { 1, 1, 1 });

            VertexCoords = Parsuj("v");
            VertexTextureCoords = VertexTexture();
            VertexNormalsCoords = Parsuj("vn");
            Sciany = Powierzchnie();
            ScianyTrojkatne = PowierzchnieTrojkaty();
        }

        public void Przesun(double tx, double ty, double tz)
        {
            VertexCoords = Przeksztalcenie3d.Translacja(VertexCoords, tx, ty, tz);
        }

        public void Obroc(double phiX, double phiY, double phiZ)
        {
            VertexCoords = Przeksztalcenie3d.Rotacja(VertexCoords, phiX, phiY, phiZ);
        }

        public void Skaluj(double sx, double sy, double sz)
        {
            VertexCoords = Przeksztalcenie3d.Skalowanie(VertexCoords, sx, sy, sz);
        }

        private List<Sciana> Powierzchnie()
        {
            string line;
            var indices = new List<Sciana>();

            using (var streamReader = new StreamReader(sciezka, true))
            {
                while ((line = streamReader.ReadLine()) != null)
                {
                    var splittedLine = line.Split(null);

                    if (splittedLine[0] == "f")
                    {
                        splittedLine = splittedLine.Skip(1).ToArray();

                        var F = new Sciana()
                        {
                            Vertex = new List<int>(),
                            VertexTexture = new List<int>(),
                            VertexNormal = new List<int>()
                        };

                        foreach (var wartosc in splittedLine)
                        {
                            F.Vertex.Add(int.Parse(wartosc.Split('/')[0]) - 1);

                            if (int.TryParse(wartosc.Split('/')[1], out int vt) == false)
                            {
                                F.VertexTexture.Add(-1);
                            }
                            else
                            {
                                F.VertexTexture.Add(vt - 1);
                            }

                            if (wartosc.Split('/').ToArray().Length == 3)
                            {
                                F.VertexNormal.Add(int.Parse(wartosc.Split('/')[2]) - 1);
                            }
                            indices.Add(F);
                        }
                    }
                }
            }
            return indices;
        }

        public List<Sciana> PowierzchnieTrojkaty()
        {
            string line;
            var indices = new List<Sciana>();

            using (var streamReader = new StreamReader(sciezka, true))
            {
                while ((line = streamReader.ReadLine()) != null)
                {
                    var splittedLine = line.Split(null);

                    if (splittedLine[0] == "f")
                    {
                        splittedLine = splittedLine.Skip(1).ToArray();

                        for (int i = 0; i < splittedLine.Length; i += 2)
                        {
                            var F = new Sciana()
                            {
                                Vertex = new List<int>(),
                                VertexTexture = new List<int>(),
                                VertexNormal = new List<int>()
                            };

                            for (int j = 0; j < 3; ++j)
                            {
                                F.Vertex.Add(int.Parse(splittedLine[(i + j) % splittedLine.Length].Split('/')[0]) - 1);

                                if (int.TryParse(splittedLine[(i + j) % splittedLine.Length].Split('/')[1], out int vt) == false)
                                {
                                    F.VertexTexture.Add(-1);
                                }
                                else
                                {
                                    F.VertexTexture.Add(vt - 1);
                                }

                                if (splittedLine[(i + j) % splittedLine.Length].Split('/').ToArray().Length == 3)
                                {
                                    F.VertexNormal.Add(int.Parse(splittedLine[(i + j) % splittedLine.Length].Split('/')[2]) - 1);
                                }
                            }
                            indices.Add(F);
                        }
                    }
                }
            }
            return indices;
        }

        private List<DenseVector> Parsuj(string typ)
        {
            string linia;
            var wierzcholki = new List<DenseVector>();

            using (var streamReader = new StreamReader(sciezka))
            {
                while ((linia = streamReader.ReadLine()) != null)
                {
                    var wartosci = linia.Split(null);
                    if (wartosci[0] == typ)
                    {
                        var wierzcholek = new List<double>();

                        foreach (var wartosc in wartosci.Skip(1))
                        {
                            wierzcholek.Add(double.Parse(wartosc, CultureInfo.InvariantCulture) * 100);
                        }
                        wierzcholki.Add(wierzcholek.ToArray());
                    }
                }
            }
            return wierzcholki;
        }

        public List<DenseVector> VertexTexture()
        {
            string linia;
            var punkty = new List<DenseVector>();

            using (var streamReader = new StreamReader(sciezka))
            {
                while ((linia = streamReader.ReadLine()) != null)
                {
                    var wartosci = linia.Split(null);
                    if (wartosci[0] == "vt")
                    {
                        punkty.Add(new DenseVector(new double[] { double.Parse(wartosci[1], CultureInfo.InvariantCulture),
                            double.Parse(wartosci[2], CultureInfo.InvariantCulture) }));
                    }
                }
            }
            return punkty;
        }
    }
}
