using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB;

using System.Collections.Generic;
using System.Linq;


namespace PlantillaDeAplicacionREVIT
{

    public class MainViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        private UIApplication revitApplication;
        private MyRelayToLaunchRevitCommands commandRelay;
        private ExternalEvent commandRelayLauncher;

        private string _windowTitleText;
        public string windowTitleText
        {
            get { return _windowTitleText; }
            set
            {
                _windowTitleText = value;
                NotifyPropertyChanged("windowTitleText");
            }
        }

        public MainViewModel(UIApplication revitApplication, MyRelayToLaunchRevitCommands commandRelay, ExternalEvent commandRelayLauncher)
        {
            windowTitleText = "PlantillaDeAplicacionREVIT"
                              + "   ["
                              + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Major.ToString()
                              + "." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Minor.ToString()
                              + "c" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Build.ToString()
                              + "]";

            this.revitApplication = revitApplication;
            this.commandRelay = commandRelay;
            this.commandRelayLauncher = commandRelayLauncher;
        }
        public MainViewModel() { }  //This empty constructor is for TEST PURPOSES ONLY; do not use for any other task!




        private string _aboutPickedElement;
        public string aboutPickedElement
        {
            get { return _aboutPickedElement; }
            set
            {
                _aboutPickedElement = value;
                NotifyPropertyChanged("aboutPickedElement");
            }
        }

        private MyCommand _AskUserToPickAnElement;
        public MyCommand AskUserToPickAnElement
        {
            get { return _AskUserToPickAnElement ?? (_AskUserToPickAnElement = new MyCommand(() => work_AskUserToPickAnElement())); }
        }
        public void work_AskUserToPickAnElement()
        {
            FamilyInstance aFamilyInstance = AskUserToPickAGenericFamilyInstance();
            if (aFamilyInstance != null)
            {
                aboutPickedElement = aFamilyInstance.Name + " located at " + ((LocationPoint)aFamilyInstance.Location).Point.ToString();
            }
        }


        private FamilyInstance AskUserToPickAGenericFamilyInstance()
        {
            Reference selectedElement = null;
            try
            {
                selectedElement = revitApplication.ActiveUIDocument.Selection.PickObject(ObjectType.Element, new GenericModelsSelectionFilter());
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException) //"Operation cancelled by user."
            { }
            catch (Autodesk.Revit.Exceptions.InvalidOperationException) //"Cannot reenter the pick operation." 
            { }

            if (selectedElement != null)
            {
                FamilyInstance familyInstance = (FamilyInstance)revitApplication.ActiveUIDocument.Document.GetElement(selectedElement.ElementId);
                return familyInstance;
            }
            else
            {
                return null;
            }
        }
        public class GenericModelsSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element element)
            {
                if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_GenericModel)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                return false;
            }
        }




        private void work_SearchForReferencePointsInside(FamilyInstance familyInstance)
        {
            //note: only placement or shape handle points are seen as ReferencePoints; regular reference points are seen as mere Points.
            List<ElementId> referencePointsIds = AdaptiveComponentInstanceUtils.GetInstancePointElementRefIds(familyInstance).ToList();

            List<ReferencePoint> referencePoints = new List<ReferencePoint>();
            foreach (ElementId id in referencePointsIds)
            {
                ReferencePoint referencePoint = (ReferencePoint)revitApplication.ActiveUIDocument.Document.GetElement(id);
                referencePoints.Add(referencePoint);
            }

            aboutPickedElement = "(point coordinates in mm) => ";
            foreach (ReferencePoint point in referencePoints)
            {
                aboutPickedElement = aboutPickedElement + " : " + point.Position.Multiply(UnitUtils.ConvertFromInternalUnits(1, DisplayUnitType.DUT_MILLIMETERS)).ToString();
            }
        }

        private void work_SearchForGeometryInsideAFamily(FamilyInstance familyInstance)
        {
            Options options = revitApplication.ActiveUIDocument.Document.Application.Create.NewGeometryOptions();
            options.ComputeReferences = true;
            options.DetailLevel = ViewDetailLevel.Undefined;
            options.IncludeNonVisibleObjects = true;
            GeometryElement gElement = familyInstance.get_Geometry(options);

            GeometryInstance gInstance = (GeometryInstance)gElement.First();
            if (gInstance != null)
            {
                GeometryElement gAsInsertedInTheProject = gInstance.GetInstanceGeometry();
                GeometryElement gAsDefinedLocallyInTheFamily = gInstance.GetSymbolGeometry();
                if (gAsInsertedInTheProject != null)
                {
                    //Outside the family file (the Symbol), that is to say, in the Instance,...
                    //...ReferencePoints appear as mere Points ?!? (in other words, you cannot access to ReferencePlanes ?!?)
                    List<Point> points = new List<Point>();
                    points = gAsInsertedInTheProject.OfType<Point>().Select<Point, Point>(x => x).ToList();

                    aboutPickedElement = "(point coordinates in mm) => ";
                    foreach (Point point in points)
                    {
                        aboutPickedElement = aboutPickedElement + " : " + point.Coord.Multiply(UnitUtils.ConvertFromInternalUnits(1, DisplayUnitType.DUT_MILLIMETERS)).ToString();
                    }
                }
            }
        }




        public ReferencePoint AskUserToPickAnAdaptivePoint()
        {
            Reference selectedElement = null;
            try
            {
                selectedElement = revitApplication.ActiveUIDocument.Selection.PickObject(ObjectType.Element, new AdaptivePointsSelectionFilter());
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException) //"Operation cancelled by user."
            { }
            catch (Autodesk.Revit.Exceptions.InvalidOperationException) //"Cannot reenter the pick operation." 
            { }

            if (selectedElement != null)
            {
                ReferencePoint referencePoint = (ReferencePoint)revitApplication.ActiveUIDocument.Document.GetElement(selectedElement.ElementId);
                return referencePoint;
            }
            else
            {
                return null;
            }
        }
        public class AdaptivePointsSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element element)
            {
                if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_AdaptivePoints)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                return false;
            }
        }



        private MyCommand _InsertAnElement;
        public MyCommand InsertAnElement
        {
            get { return _InsertAnElement ?? (_InsertAnElement = new MyCommand(() => work_InsertAnElement())); }
        }
        public void work_InsertAnElement()
        {
            ReferencePoint insertionReference = AskUserToPickAnAdaptivePoint();
            
            if(insertionReference != null)
            {
                commandRelay.parameter_a_CommandToBeCarriedOut = MyRelayToLaunchRevitCommands.AvailableCommands.InsertFamily;
                commandRelay.parameter_insertionReference = insertionReference;
                commandRelay.parameter_pathToFamilyFile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
                                                                                 , "tabla simple.rfa");
                commandRelayLauncher.Raise();
            }
        }


        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }




    }
}
