using System;
using Autodesk.Revit.DB;

namespace PlantillaDeAplicacionREVIT
{


    public class MyCommand : System.Windows.Input.ICommand
    {
        private Action _workToCarryOn;

        public MyCommand(Action workToCarryOn)
        {
            _workToCarryOn = workToCarryOn;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _workToCarryOn();
        }
    }


    public class MyFailureWarningsProcessor : Autodesk.Revit.DB.IFailuresPreprocessor
    {
        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            failuresAccessor.DeleteAllWarnings();

            return FailureProcessingResult.Continue;
        }
    }



    class MyFamilyLoadOptions : IFamilyLoadOptions
    {
        public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
        {
            overwriteParameterValues = true;

            return true;
        }

        public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
        {
            source = FamilySource.Family;
            //source = FamilySource.Project;

            overwriteParameterValues = true;

            return true;
        }
    }



}


