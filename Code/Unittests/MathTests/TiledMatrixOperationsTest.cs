using TestHelpers;
using TiledMatrixInversion.Math.MatrixOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TiledMatrixInversion.Math;

namespace TiledMatrixInversion.Tests.MathTests
{


    /// <summary>
    ///This is a test class for TiledMatrixOperationsTest and is intended
    ///to contain all TiledMatrixOperationsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class TiledMatrixOperationsTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            Matrix<double>.MatrixOperations = new DoubleMatrixOperations();
            Matrix<Matrix<double>>.MatrixOperations = new TiledMatrixOperations<double>();
        }
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        [TestInitialize()]
        public void MyTestInitialize()
        {
            var data = new Matrix<double>(new double[,]
                                   {
                                       {-29, -74, -54, -88, -51, 35, 69, -3, -26},
                                       {9, 13, -17, 91, 51, -26, -15, -62, -20},
                                       {81, 32, -25, -62, 38, -86, 2, -83, -78},
                                       {35, 48, 78, -94, -38, 50, -88, 9, -4},
                                       {80, -60, 23, 27, -19, -94, 99, 88, 5},
                                       {20, 51, -67, 18, -55, -97, -59, 95, -91},
                                       {39, 20, 28, 18, 71, -38, 10, 63, -44},
                                       {-35, -46, -81, 63, -50, -36, -44, 10, -38},
                                       {26, 35, -36, 86, -17, -69, 26, -61, -38}
                                   });
            tiledData = MatrixHelpers.Tile(data, 3);
        }
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        private static Matrix<Matrix<double>> tiledData;

        
        [TestMethod()]
        public void PlusMultiplyTest()
        {
            var a = tiledData;
            var actualTiled = a.PlusMultiply(a, a);
            var actualUntiled = MatrixHelpers.Untile(actualTiled);
            var expected = new Matrix<double>(new double[,] { { -8568, 611, -3151, 5420, 3481, 1867, -154, 13102, 1398 }, { 6298, 806, 15036, -12005, -1013, 7873, 1010, 3008, 6845 }, { -3900, -14860, 7069, -9323, 8571, 14031, 20997, -1565, 13420 }, { -3411, -1418, -19183, 3707, -1497, -9363, 1943, -14196, -8640 }, { -2461, -10275, -421, -10886, 764, 7267, 5102, -1888, -2208 }, { -19230, -12428, 228, -773, -2716, 25619, -8020, -8675, 14519 }, { 3947, -11282, -774, -3424, -1469, -3301, 6328, 3126, -1063 }, { -11564, 872, 10118, -7371, -6131, 22177, -12169, 2083, 13884 }, { -898, 1039, 16842, -13213, 5466, 19448, -2001, -3252, 9862 } });
            
            // test dimensions of tiled data
            MatrixHelpers.CompareDimensions(a, actualTiled);

            // test data in untiled output
            MatrixHelpers.Compare(expected, actualUntiled);
        }

        [TestMethod()]
        public void MinusMatrixInverseMatrixMultiplyTest()
        {
            var a = tiledData;
            var b = MatrixHelpers.Tile(new Matrix<double>(new double[,]
                                           {
                                               {2900, -74, -54, -88, -51, 35, 69, -3, -26},
                                               {9, 1300, -17, 91, 51, -26, -15, -62, -20},
                                               {81, 32, 2500, -62, 38, -86, 2, -83, -78},
                                               {35, 48, 78, 9400, -38, 50, -88, 9, -4},
                                               {80, -60, 23, 27, 1900, -94, 99, 88, 5},
                                               {20, 51, -67, 18, -55, 9700, -59, 95, -91},
                                               {39, 20, 28, 18, 71, -38, 1000, 63, -44},
                                               {-35, -46, -81, 63, -50, -36, -44, 1000, -38},
                                               {26, 35, -36, 86, -17, -69, 26, -61, 3800}
                                           }), 3);
            var actualTiled = a.MinusMatrixInverseMatrixMultiply(b);
            var actualUntiled = MatrixHelpers.Untile(actualTiled);
            var expected = new Matrix<double>(new double[,] { { 0.009340885972, 0.05928694883, 0.02283428681, 0.008948434736, 0.02812777139, -0.003240445142, -0.07067420888, 0.01122300019, 0.006875463046 }, { -0.002068964281, -0.009513298996, 0.009247416652, -0.009998571426, -0.02594475099, 0.002901263311, 0.01946740813, 0.06342939683, 0.006341543925 }, { -0.02709975994, -0.02555894223, 0.01243863111, 0.005933690788, -0.01770192777, 0.009290013085, 0.005416856153, 0.08396558691, 0.02161608163 }, { -0.01299021242, -0.03826018236, -0.03398016363, 0.009951235408, 0.01764389464, -0.005125871291, 0.08628392053, -0.02077864632, 0.0007206929450 }, { -0.02733171474, 0.04381657437, -0.01082011462, -0.002899035100, 0.01009320923, 0.009202166034, -0.1006632427, -0.08185770775, -0.003274395444 }, { -0.01028276572, -0.04458822220, 0.02284921857, -0.001163280846, 0.02530571557, 0.01037047046, 0.05193716373, -0.1009135401, 0.02391741434 }, { -0.01269930310, -0.01993415066, -0.01285870048, -0.001493084704, -0.03804778607, 0.003249650324, -0.008532751370, -0.06110788036, 0.01053963177 }, { 0.009706076909, 0.03503514849, 0.03237745268, -0.006930986595, 0.02332450662, 0.004523406400, 0.04043056111, -0.009385439307, 0.01136002205 }, { -0.008640633270, -0.02497127671, 0.01685300736, -0.009386071619, 0.01172256443, 0.007594846642, -0.02492653596, 0.06137527099, 0.01063708401 } });
            var delta = 0.0000000001;

            // test dimensions of tiled data
            MatrixHelpers.CompareDimensions(a, actualTiled);

            // test data in untiled output
            MatrixHelpers.Compare(expected, actualUntiled, delta);
        }

        [TestMethod()]
        public void UnaryMinusTest()
        {
            var a = tiledData;
            var actualTiled = -a;
            var actualUntiled = MatrixHelpers.Untile(actualTiled);
            var expected =
                new Matrix<double>(new double[,]{{29,74,54,88,51,-35,-69,3,26},{-9,-13,17,-91,-51,26,15,62,20},{-81,-32,25,62,-38,86,-2,83,78},{-35,-48,-78,94,38,-50,88,-9,4},{-80,60,-23,-27,19,94,-99,-88,-5},{-20,-51,67,-18,55,97,59,-95,91},{-39,-20,-28,-18,-71,38,-10,-63,44},{35,46,81,-63,50,36,44,-10,38},{-26,-35,36,-86,17,69,-26,61,38}});

            // test dimensions of tiled data
            MatrixHelpers.CompareDimensions(a, actualTiled);

            // test data in untiled output
            MatrixHelpers.Compare(expected, actualUntiled);       
        }

        [TestMethod()]
        public void MultiplyTest()
        {
            var a = tiledData;
            var actualTiled = a * a;
            var actualUntiled = MatrixHelpers.Untile(actualTiled);
            var expected =
                new Matrix<double>(new double[,]
                                       {
                                           {-8539, 685, -3097, 5508, 3532, 1832, -223, 13105, 1424},
                                           {6289, 793, 15053, -12096, -1064, 7899, 1025, 3070, 6865},
                                           {-3981, -14892, 7094, -9261, 8533, 14117, 20995, -1482, 13498},
                                           {-3446, -1466, -19261, 3801, -1459, -9413, 2031, -14205, -8636},
                                           {-2541, -10215, -444, -10913, 783, 7361, 5003, -1976, -2213},
                                           {-19250, -12479, 295, -791, -2661, 25716, -7961, -8770, 14610},
                                           {3908, -11302, -802, -3442, -1540, -3263, 6318, 3063, -1019},
                                           {-11529, 918, 10199, -7434, -6081, 22213, -12125, 2073, 13922},
                                           {-924, 1004, 16878, -13299, 5483, 19517, -2027, -3191, 9900}
                                       });

            // test dimensions of tiled data
            MatrixHelpers.CompareDimensions(a, actualTiled);

            // test data in untiled output
            MatrixHelpers.Compare(expected, actualUntiled);
        }

        [TestMethod()]
        public void MinusPlusPlusTest()
        {
            var data = new Matrix<double>(new double[,]
                                              {
                                                  {2900, -74, -54, -88, -51, 35, 69, -3, -26},
                                                  {9, 1300, -17, 91, 51, -26, -15, -62, -20},
                                                  {81, 32, 2500, -62, 38, -86, 2, -83, -78},
                                                  {35, 48, 78, 9400, -38, 50, -88, 9, -4},
                                                  {80, -60, 23, 27, 1900, -94, 99, 88, 5},
                                                  {20, 51, -67, 18, -55, 9700, -59, 95, -91},
                                                  {39, 20, 28, 18, 71, -38, 1000, 63, -44},
                                                  {-35, -46, -81, 63, -50, -36, -44, 1000, -38},
                                                  {26, 35, -36, 86, -17, -69, 26, -61, 3800}
                                              });
            var td1 = MatrixHelpers.Tile(data, 3);
            var td2 = MatrixHelpers.Tile(data, 3);
            var td3 = MatrixHelpers.Tile(data, 3);
            var actualTiled = td1.MinusPlusPlus(td2, td3);
            var actualUntiled = MatrixHelpers.Untile(actualTiled);
            var tiledExpected = MatrixHelpers.Tile(data, 3);
            var expected = data;
            
            // test dimensions of tiled data
            MatrixHelpers.CompareDimensions(tiledExpected, actualTiled);

            // test data in untiled output
            MatrixHelpers.Compare(expected, actualUntiled);
        }

        [TestMethod()]
        public void UFactorTest()
        {
            var a = tiledData;
            var actualTiled = a.GetU();
            var actualUntiled = MatrixHelpers.Untile(actualTiled);
            var expected =
                new Matrix<double>(new double[,]
                                       {
                                           {-29.0, -74.0, -54.0, -88.0, -51.0, 35.0, 69.0, -3.0, -26.0},
                                           {
                                               0.0, -9.965517241, -33.75862069, 63.68965517, 35.17241379, -15.13793103,
                                               6.413793103, -62.93103448, -28.06896552
                                           },
                                           {
                                               0.0, 0.0, 415.9411765, -1424.235294, -721.0, 277.1176471, 82.29411765,
                                               1011.764706, 341.4117647
                                           },
                                           {
                                               0.0, 0.0, 0.0, 58.87610538, 19.45817631, 53.21245019, -61.53665760,
                                               -105.3549461, -44.41899390
                                           },
                                           {
                                               0.0, 0.0, 0.0, 0.0, -0.08018105724, -766.9858431, 728.8115404, 1181.476831,
                                               595.9217984
                                           },
                                           {
                                               0.0, 0.0, 0.0, 0.0, 0.0, 1.327798307E6, -1.261779877E6, -2.045178533E6,-1.031700822E6
                                           },
                                           {0.0, 0.0, 0.0, 0.0, 0.0, 0.0, -10.44258292, 124.1907590, -66.15370464},
                                           {0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, -1043.738468, 517.4224166},
                                           {0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, -54.51689361}
                                       });
            var delta = 0.001;

            // test dimensions of tiled data
            MatrixHelpers.CompareDimensions(a, actualTiled);

            // test data in untiled output
            MatrixHelpers.Compare(expected, actualUntiled, delta);
        }

        [TestMethod()]
        public void LUFactorTest()
        {
            #region data
            var data =
                MatrixHelpers.Tile(
                    new Matrix<double>(new double[,]
                                           {
                                               {2900, -74, -54, -88, -51, 35, 69, -3, -26},
                                               {9, 1300, -17, 91, 51, -26, -15, -62, -20},
                                               {81, 32, 2500, -62, 38, -86, 2, -83, -78},
                                               {35, 48, 78, 9400, -38, 50, -88, 9, -4},
                                               {80, -60, 23, 27, 1900, -94, 99, 88, 5},
                                               {20, 51, -67, 18, -55, 9700, -59, 95, -91},
                                               {39, 20, 28, 18, 71, -38, 1000, 63, -44},
                                               {-35, -46, -81, 63, -50, -36, -44, 1000, -38},
                                               {26, 35, -36, 86, -17, -69, 26, -61, 3800}
                                           }), 3);

            var expected =
                new Matrix<double>(new double[,]
                                       {
                                           {2900.0, -74.0, -54.0, -88.0, -51.0, 35.0, 69.0, -3.0, -26.0},
                                           {
                                               0.00310344827600000017, 1300.22965500000009, -16.8324137900000004,
                                               91.2731034499999936, 51.1582758600000034, -26.1086206899999986,
                                               -15.2141379299999997, -61.9906896600000010, -19.9193103399999992
                                           },
                                           {
                                               0.0279310344799999986, 0.0262006764900000015, 2501.94929599999978,
                                               -61.9334860200000038, 38.0841013200000020, -86.2935226799999953,
                                               0.471379326599999982, -81.2920088900000053, -76.7518937000000108
                                           },
                                           {
                                               0.0120689655200000006, 0.0376034366300000006, 0.0316891636699999994,
                                               9399.59250699999938, -40.5150630600000028, 53.2939296300000010,
                                               -88.2755923699999983, 13.9433456400000004, -0.504969051100000010
                                           },
                                           {
                                               0.0275862069000000002, -0.0445756797299999980, 0.00948833732100000036,
                                               0.00362609257999999989, 1903.47286800000006, -95.5037934300000018,
                                               96.7339940500000070, 86.0402476299999961, 5.55940350300000042
                                           },
                                           {
                                               0.00689655172399999962, 0.0396163436400000028, -0.0263637427099999988,
                                               0.00142114580200000001, -0.0292167875699999996, 9695.65187599999990,
                                               -55.9089971900000010, 97.8273766199999956, -91.8918810400000014
                                           },
                                           {
                                               0.0134482758599999992, 0.0161472800799999997, 0.0115901647699999996,
                                               0.00196045256300000004, 0.0370364206900000020, -0.00346715334600000008,
                                               995.708806699999968, 62.1087380700000011, -42.9626487299999980
                                           },
                                           {
                                               -0.0120689655200000006, -0.0360652468300000015, -0.0328778802200000026,
                                               0.00672300168900000044, -0.0248209337899999994, -0.00434061961100000015,
                                               -0.0411250954699999977, 1000.07609200000002, -43.5799537500000014
                                           },
                                           {
                                               0.00896551724100000044, 0.0274285762799999985, -0.0140107447300000000,
                                               0.00887461237399999986, -0.00895879078500000024, -0.00733682167099999981,
                                               0.0271616787800000002, -0.0607292884500000020, 3797.60456100000010
                                           }
                                       });
            #endregion

            var a = data;
            var actualTiled = a.GetLU();
            var actualUntiled = MatrixHelpers.Untile(actualTiled);

            var delta = 0.0001;

            // test dimensions of tiled data
            MatrixHelpers.CompareDimensions(a, actualTiled);

            // test data in untiled output
            MatrixHelpers.Compare(expected, actualUntiled, delta);
        }

        [TestMethod()]
        public void LFactorTest()
        {
            var a = tiledData;
            var actualTiled = a.GetL();
            var actualUntiled = MatrixHelpers.Untile(actualTiled);
            var expected =
                new Matrix<double>(new double[,]
                                       {
                                           {1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0},
                                           {-0.3103448276, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0},
                                           {-2.793103448, 17.52941176, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0},
                                           {-1.206896552, 4.145328720, 0.3672831033, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0},
                                           {-2.758620690, 26.50519031, 1.848369895, 12.37596449, 1.0, 0.0, 0.0, 0.0, 0.0},
                                           {-0.6896551724, 0.003460207612, -0.2503348391, -6.784513771, 1730.723892, 1.0,0.0, 0.0, 0.0},
                                           {-1.344827586, 7.979238754, 0.5403345895, 2.734960126, -724.9668509,-0.4188924783, 1.0, 0.0, 0.0},
                                           {1.206896552, -4.346020761, -0.3907842305, -1.877930488, 1007.751279,0.5821630828, 7.843129276, 1.0, 0.0},
                                           {-0.8965517241, 3.145328720, 0.05233472260, -2.015834666, 1202.218369,0.6945235749, -8.099897990, -0.8684269814, 1.0}
                                       });
            var delta = 0.0001;

            // test dimensions of tiled data
            MatrixHelpers.CompareDimensions(a, actualTiled);

            // test data in untiled output
            MatrixHelpers.Compare(expected, actualUntiled, delta);
        }

        [TestMethod()]
        public void InverseTest()
        {
            var data = tiledData;            
            var expected = new Matrix<double>(new double[,] { { 0.02192015945, 0.07998061455, -0.008445925650, 0.008710218509, 0.01971795582, 0.03369912515, -0.03788966209, -0.04047526651, -0.03443211738 }, { 0.001285473093, 0.006475451796, -0.003107785784, -0.002406763251, -0.002156769941, 0.01009704045, -0.006583144946, -0.01506901599, 0.0005728805958 }, { -0.01701324231, -0.06216985719, 0.005607568490, 0.003398502231, -0.01075343654, -0.03286856797, 0.03576146110, 0.03544270502, 0.03293953991 }, { 0.003728644331, 0.01491375624, -0.006042286532, 0.002970447471, 0.003023926186, 0.003526862044, -0.002322515396, -0.003817996172, 0.0001485894513 }, { -0.0005615412594, 0.007838447265, 0.002333491962, -0.004784087453, -0.0007533342828, 0.002142041231, 0.0004894987124, -0.004087439707, -0.009735593520 }, { 0.02230167876, 0.05874030688, -0.01264627160, 0.008130435740, 0.008704109428, 0.02306863101, -0.02105124873, -0.03019428258, -0.02060130358 }, { 0.003804072365, -0.005281366040, -0.002279134971, -0.002468192455, -0.0007855692333, -0.001365582163, 0.003990170913, -0.004396507733, 0.008058069508 }, { 0.003848095322, 0.01266542130, -0.004489737629, -0.0005820329480, 0.003474043788, 0.009709450206, -0.004204219769, -0.008854978380, -0.009093319451 }, { -0.003080045444, 0.01922614978, -0.00006146877238, -0.006131531678, 0.006953761372, 0.01211106349, -0.02363878097, -0.01592950229, -0.01834293801 } });
            var actualTiled = data.Inverse();
            //var actualUntiled = MatrixHelpers.Untile(actualTiled);

            //var delta = 0.00001;

            //// test dimensions of tiled data
            //MatrixHelpers.CompareDimensions(data, actualTiled);

            //// test data in untiled output
            //MatrixHelpers.Compare(expected, actualUntiled, delta);
        }

        [TestMethod]
        public void Inverse_MAPLE_100x100Test()
        {
            var tilesize = 30;
            var delta = 1.0E-11;

            var data = Matrix<double>.ReadFromFile(@"C:\Users\eh\Documents\KU\Inversion-of-Block-Tridiagonal-Matrices\Dataset\Maple Test Data\a-100x100.csv");
            var expected = Matrix<double>.ReadFromFile(@"C:\Users\eh\Documents\KU\Inversion-of-Block-Tridiagonal-Matrices\Dataset\Maple Test Data\a-inverse-100x100.csv");

            var tiledData = MatrixHelpers.Tile(data, tilesize);

            var actualTiled = tiledData.Inverse();
            var actual = MatrixHelpers.Untile(actualTiled);

            MatrixHelpers.Diff(expected, actual, delta);
            MatrixHelpers.Compare(expected, actual, delta);
        }

        //[TestMethod()]
        //public void Inverse_MAPLE_1000x1000Test()
        //{
        //    Assert.Inconclusive("Missing test data");
        //    var tilesize = 30;
        //    var delta = 1.0E-11;

        //    var data =
        //        Matrix<double>.ReadFromFile(
        //            @"C:\Users\eh\Documents\KU\Inversion-of-Block-Tridiagonal-Matrices\Dataset\Maple Test Data\b-1000x1000.csv");
        //    var expected =
        //        Matrix<double>.ReadFromFile(
        //            @"C:\Users\eh\Documents\KU\Inversion-of-Block-Tridiagonal-Matrices\Dataset\Maple Test Data\b-inverse-1000x1000.csv");

        //    var tiledData = MatrixHelpers.Tile(data, tilesize);

        //    var actualTiled = tiledData.Inverse();
        //    var actual = MatrixHelpers.Untile(actualTiled);

        //    MatrixHelpers.Diff(expected, actual, delta);
        //    MatrixHelpers.Compare(expected, actual, delta);
        //}

        [TestMethod()]
        public void ComputeLUTest()
        {
            // calls the same function in the backend
            LUFactorTest();
        }

        [TestMethod()]
        public void AdditionTest()
        {
            var a = tiledData;
            var actualTiled = a + a;
            var actualUntiled = MatrixHelpers.Untile(actualTiled);
            var expected =
                new Matrix<double>(new double[,]
                                       {
                                           {-58, -148, -108, -176, -102, 70, 138, -6, -52},
                                           {18, 26, -34, 182, 102, -52, -30, -124, -40},
                                           {162, 64, -50, -124, 76, -172, 4, -166, -156},
                                           {70, 96, 156, -188, -76, 100, -176, 18, -8},
                                           {160, -120, 46, 54, -38, -188, 198, 176, 10},
                                           {40, 102, -134, 36, -110, -194, -118, 190, -182},
                                           {78, 40, 56, 36, 142, -76, 20, 126, -88},
                                           {-70, -92, -162, 126, -100, -72, -88, 20, -76},
                                           {52, 70, -72, 172, -34, -138, 52, -122, -76}
                                       });

            // test dimensions of tiled data
            MatrixHelpers.CompareDimensions(a, actualTiled);

            // test data in untiled output
            MatrixHelpers.Compare(expected, actualUntiled);

        }

        [TestMethod()]
        [DeploymentItem("TiledMatrixInversion.Math.dll")]
        public void TiledCloneTest()
        {
            var actual = tiledData.Clone();
            var expected = tiledData;

            // test data in untiled output
            MatrixHelpers.Compare(expected, actual);
        }


        [TestMethod]
        public void GetUpperTriangelTest()
        {
            var a = tiledData;
            var actualTiled = a.GetUpperTriangle();
            var expected = MatrixHelpers.Untile(tiledData).GetUpperTriangle();
            var actualUntiled = MatrixHelpers.Untile(actualTiled);

            // test dimensions of tiled data
            MatrixHelpers.CompareDimensions(a, actualTiled);

            // test data in untiled output
            MatrixHelpers.Compare(expected, actualUntiled);
        }

        [TestMethod]
        public void GetLowerTriangelTest()
        {
            var a = tiledData;
            var actualTiled = a.GetLowerTriangle();
            var expected = MatrixHelpers.Untile(tiledData).GetLowerTriangle();
            var actualUntiled = MatrixHelpers.Untile(actualTiled);

            // test dimensions of tiled data
            MatrixHelpers.CompareDimensions(a, actualTiled);

            // test data in untiled output
            MatrixHelpers.Compare(expected, actualUntiled);
        }

        [TestMethod]
        public void GetLowerTriangelWithFixedDiagonalTest()
        {
            var a = tiledData;
            var actualTiled = a.GetLowerTriangleWithFixedDiagonal();
            var expected = MatrixHelpers.Untile(tiledData).GetLowerTriangleWithFixedDiagonal();
            var actualUntiled = MatrixHelpers.Untile(actualTiled);

            // test dimensions of tiled data
            MatrixHelpers.CompareDimensions(a, actualTiled);

            // test data in untiled output
            MatrixHelpers.Compare(expected, actualUntiled);
        }
    }
}
