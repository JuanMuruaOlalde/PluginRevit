using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.Attributes;
//using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace PlantillaDeAplicacionREVIT
{
    class CodeSamples
    {
        // this is to fullfil the Autodesk.Revit.UI.IExternalCommand interface
        // copied from https://knowledge.autodesk.com/support/revit-products/learn-explore/caas/simplecontent/content/lesson-1-the-basic-plug.html
        //
        public Autodesk.Revit.UI.Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            //Get application and document objects

            UIApplication uiapp = commandData.Application;

            Document doc = uiapp.ActiveUIDocument.Document;

            //Define a reference Object to accept the pick result

            Reference pickedref = null;

            //Pick a group

            Selection sel = uiapp.ActiveUIDocument.Selection;

            pickedref = sel.PickObject(ObjectType.Element, "Please select a group");

            Element elem = doc.GetElement(pickedref);

            Group group = elem as Group;

            //Pick point

            XYZ point = sel.PickPoint("Please pick a point to place group");

            //Place the group

            Transaction trans = new Transaction(doc);

            trans.Start("Lab");

            doc.Create.PlaceGroup(point, group.GroupType);

            trans.Commit();

            return Result.Succeeded;

        }
    }
}
