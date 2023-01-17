using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PlantillaDeAplicacionREVIT
{

//// in order to use any of these... comment or uncomment [TestClass] as need...
//    [TestClass]
    public class AuxiliaryHelperFunctions
    {

        [TestMethod]
        public void StartMainWindowOutsideRevitForVisualInspectionOfTheInterface()
        {
            MainWindow window = new MainWindow();
            window.DataContext = new MainViewModel();
            window.ShowDialog();
        }


    }
}
