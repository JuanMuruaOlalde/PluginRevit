using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using System;
using System.Linq;


namespace PlantillaDeAplicacionREVIT
{

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class Main: IExternalApplication
    {
        public static Main PlantillaDeAplicacionREVITApplication;
        private MainWindow PlantillaDeAplicacionREVITWindow;
        private MyRelayToLaunchRevitCommands commandRelay;
        private ExternalEvent commandRelayLauncher;


        public Result OnShutdown(UIControlledApplication application)
        {
            if (PlantillaDeAplicacionREVITWindow != null)
            {
                commandRelayLauncher = null;
                commandRelay = null;
                PlantillaDeAplicacionREVITWindow.Close();
            }

            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            PlantillaDeAplicacionREVITApplication = this;
            PlantillaDeAplicacionREVITWindow = null;

            try
            {
                RibbonPanel panel = application.CreateRibbonPanel("PlantillaDeAplicacionREVIT");
                PushButton btnPlantillaDeAplicacionREVIT = (PushButton)panel.AddItem(new PushButtonData(name: "StartPlantillaDeAplicacionREVIT", text: "launch PlantillaDeAplicacionREVIT",
                                                                                                        assemblyName: System.Reflection.Assembly.GetExecutingAssembly().Location,
                                                                                                        className: "PlantillaDeAplicacionREVIT.LaunchPlantillaDeAplicacionREVIT"));
                btnPlantillaDeAplicacionREVIT.LargeImage = new System.Windows.Media.Imaging.BitmapImage(new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
                                                                                                                + System.IO.Path.DirectorySeparatorChar + "PlantillaDeAplicacionREVIT.bmp"));

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog feedbackForUser = new TaskDialog(title: "PlantillaDeAplicacionREVIT.Main.OnStartup");
                feedbackForUser.CommonButtons = TaskDialogCommonButtons.Ok;
                feedbackForUser.MainContent = ex.ToString() + Environment.NewLine + ex.StackTrace;
                feedbackForUser.Show();
                return Result.Failed;
            }
        }

        public void ShowPlantillaDeAplicacionREVITWindow(UIApplication revitApplication)
        {
            if (PlantillaDeAplicacionREVITWindow == null)
            {
                commandRelay = new MyRelayToLaunchRevitCommands();
                commandRelayLauncher = ExternalEvent.Create(commandRelay);

                PlantillaDeAplicacionREVITWindow = new MainWindow();
                PlantillaDeAplicacionREVITWindow.DataContext = new MainViewModel(revitApplication, commandRelay, commandRelayLauncher);
                PlantillaDeAplicacionREVITWindow.Show();
            }
            else
            {
                PlantillaDeAplicacionREVITWindow.Visibility = System.Windows.Visibility.Visible;
                PlantillaDeAplicacionREVITWindow.Focus();
            }
        }

    }



    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class LaunchPlantillaDeAplicacionREVIT : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                Main.PlantillaDeAplicacionREVITApplication.ShowPlantillaDeAplicacionREVITWindow(commandData.Application);

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog feedbackForUser = new TaskDialog(title: "PlantillaDeAplicacionREVIT.Execute");
                feedbackForUser.CommonButtons = TaskDialogCommonButtons.Ok;
                feedbackForUser.MainContent = ex.ToString() + Environment.NewLine + ex.StackTrace;
                feedbackForUser.Show();
                return Result.Failed;
            }
        }
    }


    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class MyRelayToLaunchRevitCommands : IExternalEventHandler
    {
        public enum AvailableCommands
        {
            InsertFamily,
            zzDoNothing
        }
        public AvailableCommands parameter_a_CommandToBeCarriedOut = AvailableCommands.zzDoNothing;

        public ReferencePoint parameter_insertionReference;
        public string parameter_pathToFamilyFile;

        public void Execute(UIApplication revitApplication)
        {
            switch (parameter_a_CommandToBeCarriedOut)
            {
                case AvailableCommands.zzDoNothing:
                    break;


                case AvailableCommands.InsertFamily:

                    Document activeDocument = revitApplication.ActiveUIDocument.Document;

                    ElementId IDforReferencePlane = parameter_insertionReference.GetCoordinatePlaneReferenceXY().ElementId;

                    using (Transaction trans = new Transaction(activeDocument, "Place component from PlantillaDeAplicacionREVIT"))
                    {
                        trans.Start();
                        FailureHandlingOptions MyCustomFailureHandler = trans.GetFailureHandlingOptions();
                        MyCustomFailureHandler.SetFailuresPreprocessor(new MyFailureWarningsProcessor());
                        trans.SetFailureHandlingOptions(MyCustomFailureHandler);

                        Element familyAlreadyInTheDocument = new FilteredElementCollector(activeDocument)
                                                                     .OfClass(typeof(Family))
                                                                     .FirstOrDefault<Element>(x => x.Name.Equals(System.IO.Path.GetFileNameWithoutExtension(parameter_pathToFamilyFile)));
                        Family family;
                        if (familyAlreadyInTheDocument == null)
                        {
                            activeDocument.LoadFamily(parameter_pathToFamilyFile, new MyFamilyLoadOptions(), out family);
                        }
                        else
                        {
                            family = (Family)familyAlreadyInTheDocument;
                        }

                        if (family != null)
                        {
                            ElementId IDforFirstSymbolOnTheFamily = family.GetFamilySymbolIds().First();
                            FamilySymbol firstSymbolOnTheFamily = (FamilySymbol)family.Document.GetElement(IDforFirstSymbolOnTheFamily);
                            if (!firstSymbolOnTheFamily.IsActive)
                            {
                                firstSymbolOnTheFamily.Activate();
                                activeDocument.Regenerate();
                            }
                            FamilyInstance instancia = activeDocument.Create.NewFamilyInstance(parameter_insertionReference.Position,
                                                                                               firstSymbolOnTheFamily,
                                                                                               Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                        }

                        trans.Commit();
                    }
                    break;

            }

        }


        public string GetName()
        {
            return "External Event Handler for PlantillaDeAplicacionRevit";
        }


    }


}
