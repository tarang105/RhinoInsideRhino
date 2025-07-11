﻿using System;
using Rhino;
using Rhino.Commands;
using Rhino.Input.Custom;
using Rhino.Input;
using Rhino.DocObjects;
using RhinoInsideRhino.ObjectModel;
using System.Collections.Generic;

namespace RhinoInsideRhino.Commands
{
    public class CreateCurveHostObject : Command
    {
        public CreateCurveHostObject()
        {
            Instance = this;
        }


        ///<summary>The only instance of this command.</summary>
        public static CreateCurveHostObject Instance { get; private set; }

        public override string EnglishName => "CreateCurveHostObject";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // Setup object getter
            GetObject go = new GetObject();
            go.SetCommandPrompt("Select one or more curves");

            go.GeometryFilter = ObjectType.Curve;
            go.AcceptNothing(false);




            // set up the options
            OptionToggle boolOption = new OptionToggle(false, "No", "Yes");
            go.AddOptionToggle("KeepOriginals", ref boolOption);



            // Get options and objects
            while (true)
            {
                GetResult get_rc = go.GetMultiple(1, 0);
                if (go.CommandResult() != Result.Success)
                    return go.CommandResult();


                if (get_rc == GetResult.Option)
                {
                    continue;
                }



                break;
            }

            if (go.CommandResult() != Result.Success)
                return go.CommandResult();



            bool keepOriginal = boolOption.CurrentValue;

            
            // Collect selected curves and create curve host objects
            foreach (var obj in go.Objects())
            {
                var curve = obj.Curve();
                if (curve == null) continue;

                DateTime dateTime = DateTime.Now;
                int timeMsSinceMidnight = (int)dateTime.TimeOfDay.TotalSeconds;
                Random rand = new Random(timeMsSinceMidnight);

                CurveHostObject curveHostObjects = new CurveHostObject(curve);
                curveHostObjects.Data.Parameters = new Dictionary<string, ParameterObject>
                {
                    ["pelmet_ht"] = new SliderParameterObject { Type = "Slider", Value = 2.7 },
                    ["close_percent"] = new SliderParameterObject { Type = "Slider", Value = 62.0 },
                    ["open_location"] = new SliderParameterObject { Type = "Slider", Value = 1 }
                };

                //curveHostObjects.Data.ModelId = rand.Next().ToString()
                RhinoDoc.ActiveDoc.Objects.AddRhinoObject(curveHostObjects, curve);

                // Optionally, delete the original Brep object if KeepOriginal is false
                if (!keepOriginal)
                {
                    doc.Objects.Delete(obj, true);
                }
            }

            return Result.Success;
        }
    }
}