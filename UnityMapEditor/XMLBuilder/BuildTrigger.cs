﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;
using Entity;
using static System.Collections.Specialized.BitVector32;

namespace ExportScenario.XMLBuilder
{
    internal class BuildTrigger
    /// Class to create Tiggers. Trigger allow to define start or stop conditions for Storys or Events.
    {
        private XmlDocument root;
        private ScenarioInfo scenarioInfo;
        public BuildTrigger(XmlDocument root, ScenarioInfo scenarioInfo)
        {
            this.root = root;
            this.scenarioInfo = scenarioInfo;
        }

        public void CombineTrigger(XmlNode parentNode, bool start, List<TriggerInfo> TriggerList)
        /// Combines Trigger xmlBlock - if required - with multiple condtitions in a condition group.
        {
            XmlNode trigger;
            if (start)
            {
                trigger = root.CreateElement("StartTrigger");
            }
            else
            {
                trigger = root.CreateElement("StopTrigger");
            }

            XmlNode conditionGroup = root.CreateElement("ConditionGroup");
            for (int i = 0; i < TriggerList.Count; i++)
            {
                XmlNode condition = root.CreateElement("Condition");
                SetAttribute("name", TriggerList[i].TriggerType + TriggerList[i].ID, condition);
                SetAttribute("delay", TriggerList[i].Delay.ToString(), condition);
                SetAttribute("conditionEdge", TriggerList[i].ConditionEdge.ToString().FirstCharToLowerCase(), condition);

                // Invokes a Method for specified object with specified inputs via a String
                MethodInfo mi = this.GetType().GetMethod(TriggerList[i].TriggerType);
                mi.Invoke(this, new object[] { condition, TriggerList[i] });
                conditionGroup.AppendChild(condition);
            }
            parentNode.AppendChild(trigger);
            trigger.AppendChild(conditionGroup);
        }

        public void SimulationTimeCondition(XmlNode condition, TriggerInfo triggerInfo)
        /// Creates SimulationTimeCondition. Triggers after simulation ran for specified time.
        {
            XmlNode byValueCondition = root.CreateElement("ByValueCondition");
            condition.AppendChild(byValueCondition);
            XmlNode simulationTimeCondition = root.CreateElement("SimulationTimeCondition");
            SetAttribute("value", triggerInfo.SimulationTimeValue.ToString(), simulationTimeCondition);
            SetAttribute("rule", triggerInfo.Rule.ToString().FirstCharToLowerCase(), simulationTimeCondition);
            byValueCondition.AppendChild(simulationTimeCondition);
        }

        public void DistanceCondition(XmlNode condition, TriggerInfo triggerInfo)
        /// Create DistanceCondition. Triggers when specified entity traveled specified distance.
        {
            XmlNode byEntityCondition = root.CreateElement("ByEntityCondition");
            XmlNode triggeringEntities = root.CreateElement("TriggeringEntities");
            SetAttribute("triggeringEntitiesRule", "any", triggeringEntities);
            XmlNode entityRef = root.CreateElement("EntityRef");
            SetAttribute("entityRef", triggerInfo.EntityRef, entityRef);

            XmlNode entityCondition = root.CreateElement("EntityCondition");
            XmlNode distanceCondition = root.CreateElement("DistanceCondition");
            SetAttribute("freespace", "false", distanceCondition); // true is not implemented in carla
            SetAttribute("rule", triggerInfo.Rule.ToString().FirstCharToLowerCase(), distanceCondition);
            SetAttribute("value", triggerInfo.Value.ToString(), distanceCondition);
            SetAttribute("alongRoute", "true", distanceCondition);
            XmlNode position = root.CreateElement("Position");
            XmlNode worldposition = root.CreateElement("WorldPosition");
            SetAttribute("x", triggerInfo.WorldPositionCarla.Vector3Ser.ToVector3().x.ToString(), worldposition);
            SetAttribute("y", triggerInfo.WorldPositionCarla.Vector3Ser.ToVector3().y.ToString(), worldposition);
            SetAttribute("z", triggerInfo.WorldPositionCarla.Vector3Ser.ToVector3().z.ToString(), worldposition);
            SetAttribute("h", triggerInfo.WorldPositionCarla.Rot.ToString(), worldposition);

            // hierarchy
            condition.AppendChild(byEntityCondition);
            byEntityCondition.AppendChild(triggeringEntities);
            byEntityCondition.AppendChild(entityCondition);
            triggeringEntities.AppendChild(entityRef);
            entityCondition.AppendChild(distanceCondition);
            distanceCondition.AppendChild(position);
            position.AppendChild(worldposition);
        }

        public void ReachPositionCondition(XmlNode condition, TriggerInfo triggerInfo)
        /// Create DistanceCondition. Triggers when specified entity traveled specified distance.
        /// Same as DistanceCondition but simpler to read
        {
            XmlNode byEntityCondition = root.CreateElement("ByEntityCondition");
            XmlNode triggeringEntities = root.CreateElement("TriggeringEntities");
            SetAttribute("triggeringEntitiesRule", "any", triggeringEntities);
            XmlNode entityRef = root.CreateElement("EntityRef");
            SetAttribute("entityRef", triggerInfo.EntityRef, entityRef);

            XmlNode entityCondition = root.CreateElement("EntityCondition");
            XmlNode reachPositionCondition = root.CreateElement("ReachPositionCondition");
            SetAttribute("tolerance", "5", reachPositionCondition); 
            //SetAttribute("tolerance", triggerInfo.Value.ToString(), reachPositionCondition); 
            
            XmlNode position = root.CreateElement("Position");
            XmlNode worldposition = root.CreateElement("WorldPosition");
            SetAttribute("x", triggerInfo.WorldPositionCarla.Vector3Ser.ToVector3().x.ToString(), worldposition);
            SetAttribute("y", triggerInfo.WorldPositionCarla.Vector3Ser.ToVector3().y.ToString(), worldposition);
            SetAttribute("z", triggerInfo.WorldPositionCarla.Vector3Ser.ToVector3().z.ToString(), worldposition);
            SetAttribute("h", triggerInfo.WorldPositionCarla.Rot.ToString(), worldposition);

            // hierarchy
            condition.AppendChild(byEntityCondition);
            byEntityCondition.AppendChild(triggeringEntities);
            byEntityCondition.AppendChild(entityCondition);
            triggeringEntities.AppendChild(entityRef);
            entityCondition.AppendChild(reachPositionCondition);
            reachPositionCondition.AppendChild(position);
            position.AppendChild(worldposition);
        }

        public void StandStillCondition(XmlNode condition, TriggerInfo triggerInfo)
        /// Create StandStillCondition. Triggers when specified entity does not move for a specific time.
        {
            XmlNode byEntityCondition = root.CreateElement("ByEntityCondition");
            XmlNode triggeringEntities = root.CreateElement("TriggeringEntities");
            SetAttribute("triggeringEntitiesRule", "any", triggeringEntities);
            XmlNode entityRef = root.CreateElement("EntityRef");
            SetAttribute("entityRef", triggerInfo.EntityRef, entityRef);

            XmlNode entityCondition = root.CreateElement("EntityCondition");
            XmlNode standStillCondition = root.CreateElement("StandStillCondition");
            SetAttribute("duration", triggerInfo.Value.ToString(), standStillCondition);

            // hierarchy
            condition.AppendChild(byEntityCondition);
            byEntityCondition.AppendChild(triggeringEntities);
            byEntityCondition.AppendChild(entityCondition);
            triggeringEntities.AppendChild(entityRef);
            entityCondition.AppendChild(standStillCondition);
        }

        public void StoryboardElementStateCondition(XmlNode condition, TriggerInfo triggerInfo)
        /// Trigger that is true if another Action is completed. Useful to create follow up actions.
        {
            XmlNode byValueCondition = root.CreateElement("ByValueCondition");
            condition.AppendChild(byValueCondition);
            XmlNode storyboardElementStateCondition = root.CreateElement("StoryboardElementStateCondition");
            SetAttribute("storyboardElementType", "action", storyboardElementStateCondition);
            SetAttribute("storyboardElementRef", triggerInfo.AfterAction.Name + triggerInfo.AfterAction.ID, storyboardElementStateCondition);
            SetAttribute("state", "completeState", storyboardElementStateCondition);
            byValueCondition.AppendChild(storyboardElementStateCondition);
        }

        public void CriteriaConditions(XmlNode stopTrigger)
        {
            XmlNode conditionGroup = root.CreateElement("ConditionGroup");

            CriteriaCondition(conditionGroup, "criteria_RunningStopTest", "", "");
            CriteriaCondition(conditionGroup, "criteria_RunningRedLightTest", "", "");
            CriteriaCondition(conditionGroup, "criteria_WrongLaneTest", "", "");
            CriteriaCondition(conditionGroup, "criteria_OnSidewalkTest", "", "");
            CriteriaCondition(conditionGroup, "criteria_KeepLaneTest", "", "");
            CriteriaCondition(conditionGroup, "criteria_CollisionTest", "", "");
            CriteriaCondition(conditionGroup, "criteria_DrivenDistanceTest", "distance_success", "1");

            stopTrigger.AppendChild(conditionGroup);
        }

        private void CriteriaCondition(XmlNode conditionGroup, string name, string parameterRef, string value)
        {
            XmlNode condition = root.CreateElement("Condition");
            SetAttribute("name", name, condition);
            SetAttribute("delay", "0", condition);
            SetAttribute("conditionEdge", "rising", condition);
            XmlNode byValueCondition = root.CreateElement("ByValueCondition");
            XmlNode parameterCondition = root.CreateElement("ParameterCondition");
            SetAttribute("parameterRef", parameterRef, parameterCondition);
            SetAttribute("value", value, parameterCondition);
            SetAttribute("rule", "lessThan", parameterCondition);

            conditionGroup.AppendChild(condition);
            condition.AppendChild(byValueCondition);
            byValueCondition.AppendChild(parameterCondition);
        }


        // AccelerationCondition(XmlNode conditionGroup, Waypoint waypoint)


        // ---------------------------------------------------------------------------------------------------------------------
        // ---------------------------------------------------------------------------------------------------------------------
        // ---------------------------------------------------------------------------------------------------------------------
        // ToDo create designated methods for all relevant triggers instead of only ByValueCondition and ByEntityCondition
        // ---------------------------------------------------------------------------------------------------------------------
        // ---------------------------------------------------------------------------------------------------------------------
        // ---------------------------------------------------------------------------------------------------------------------

        /*
        public void ByValueCondition(XmlNode conditionGroup, string ValueCondition, string dict_args) // original: dict args

        {
            // TODO ScenarioInfo

            string conditionEdge = "rising"; // possible string values: "rising" , "falling" , "none" , "risingOrFalling"
            // doc: https://www.asam.net/static_downloads/ASAM_OpenSCENARIO_V1.2.0_Model_Documentation/modelDocumentation/content/ConditionEdge.html

            // SimulationTimeCondition
            double simulationTimeValue = 1;
            string simulationTimeRule = "greaterThan"; // "equalTo" , "greaterThan" , "lessThan" ,
                                                       // (not sure if supported) "greaterOrEqual" , "lessOrEqual" , "notEqualTo"

            // StoryBoardElementStateCondition
            string storyboardElementRef = "STORY_BOARD_ELEMENT_NAME"; // story, act, maneuverGroup, maneuver, event, or action name
            string storyboardElementType = "action"; // "story", "act", "maneuverGroup", "maneuver", "event", "action" 
                                               // This variable must have the same type as the storyboardElementRef name is referring to.
                                               // e.g. storyboardElementRef = "StartCar". This is of type action --> storyboardElementType = "action";
            string storyboardState = "completeState"; // "completeState", "endTransition", "runningState", "skipTransition", 
                                                      // "standbyState", "startTransition", "stopTransition"
                                                      // Doc: https://www.asam.net/static_downloads/ASAM_OpenSCENARIO_V1.2.0_Model_Documentation/modelDocumentation/content/StoryboardElementState.html

            // -----------------------------------------------------------------------------------------

            XmlNode condition = root.CreateElement("Condition");
            SetAttribute("name", "condition", condition);
            // conditionNr++; // I assume every condition needs a unique name. If not, this conditionNr can be deleted. -> does not need new name
            SetAttribute("delay", "0", condition);
            SetAttribute("conditionEdge", conditionEdge, condition);
            XmlNode byValueCondition = root.CreateElement("ByValueCondition");

            conditionGroup.AppendChild(condition);
            condition.AppendChild(byValueCondition);

            if (ValueCondition.Equals("SimulationTimeCondition"))
            {
                XmlNode simulationTimeCondition = root.CreateElement("SimulationTimeCondition");
                SetAttribute("value", simulationTimeValue.ToString(), simulationTimeCondition);
                SetAttribute("rule", simulationTimeRule, simulationTimeCondition);
                byValueCondition.AppendChild(simulationTimeCondition);
            } 
            else if (ValueCondition.Equals("StoryboardElementStateCondition"))
            {
                XmlNode storyboardElementStateCondition = root.CreateElement("StoryboardElementStateCondition");
                SetAttribute("storyboardElementRef", storyboardElementRef, storyboardElementStateCondition);
                SetAttribute("storyboardElementType", storyboardElementType, storyboardElementStateCondition);
                SetAttribute("state", storyboardState, storyboardElementStateCondition);
                byValueCondition.AppendChild(storyboardElementStateCondition);
            } else
            {
                Console.WriteLine("Naming error in value condition. This name is not supported.");
            }
        }

         <Condition>
           <ByEntityCondition>
               <TriggeringEntities>
                   <EntityRef entityRef="adversary0"/>
               </TriggeringEntities>
                   <EntityCondition>
                       //Space for entity condition
                   </EntityCondition>
           </ByEntityCondition>
         <Condition>

        public void ByEntityCondition(XmlNode conditionGroup, string EntityCondition, List<string> allEntityRefs, string dict_args) // original: Dict args
        {
            // TODO ScenarioInfo

            string conditionEdge = "rising"; // possible string values: "rising" , "falling" , "none" , "risingOrFalling"
            // doc: https://www.asam.net/static_downloads/ASAM_OpenSCENARIO_V1.2.0_Model_Documentation/modelDocumentation/content/ConditionEdge.html

            List<string> allEntityRefs_; // contains strings like "adversary0" , "adversary0" , "hero", ...

            // ReachPositionCondition
            double tolerance = 3.0;

            //DistanceCondition
            string rule = "greaterThan"; // "equalTo", "greaterThan", "lessThan", (not sure if supported) "greaterOrEqual", "lessOrEqual", "notEqualTo"
            double value = 3.0; // The distance value. Unit: [m]. Range: [0..inf[.
            string routingAlgorithm = "fastest"; // "undefined", "fastest" , "shortest" , ... (these 2 are the relevant options)

            // -----------------------------------------------------------------------

            XmlNode condition = root.CreateElement("Condition");
            SetAttribute("name", "condition", condition);
            //conditionNr++; // I assume every condition needs a unique name. If not, this conditionNr can be deleted.
            SetAttribute("delay", "0", condition);
            SetAttribute("conditionEdge", conditionEdge, condition);
            XmlNode byEntityCondition = root.CreateElement("ByEntityCondition");
            XmlNode triggeringEntities = root.CreateElement("TriggeringEntities");
            SetAttribute("triggeringEntitiesRule", "any", triggeringEntities);
            
            foreach (string r in allEntityRefs)
            {
                XmlNode entityRef = root.CreateElement("EntityRef");
                SetAttribute("entityRef", r, entityRef);
                triggeringEntities.AppendChild(entityRef);
            }

            XmlNode entityCondition = root.CreateElement("EntityCondition");

            conditionGroup.AppendChild(condition);
            condition.AppendChild(byEntityCondition);
            byEntityCondition.AppendChild(triggeringEntities);
            byEntityCondition.AppendChild(entityCondition);

            if (EntityCondition.Equals("DistanceCondition"))
            {
                //XmlNode distanceCondition = root.CreateElement("DistanceCondition");
                //SetAttribute("freespace", "false", distanceCondition); // true is not implemented in carla
                //SetAttribute("rule", rule, distanceCondition);
                //SetAttribute("value", value.ToString(), distanceCondition);
                //SetAttribute("routingAlgorithm", routingAlgorithm, distanceCondition);
                //XmlNode position = root.CreateAttribute("Position");
                //XmlNode worldposition = root.CreateElement("WorldPosition");
                //SetAttribute("x", "300.0", worldposition);
                //SetAttribute("y", "300.0", worldposition);
                //SetAttribute("z", "0.3", worldposition);
                //SetAttribute("h", "90", worldposition);
                //entityCondition.AppendChild(distanceCondition);
                //distanceCondition.AppendChild(position);
                //position.AppendChild(worldposition);
            }
            else if (EntityCondition.Equals("ReachPositionCondition"))
            {
                XmlNode reachPositionCondition = root.CreateElement("ReachPositionCondition");
                SetAttribute("tolerance", tolerance.ToString(), reachPositionCondition);
                XmlNode position = root.CreateAttribute("Position");
                XmlNode worldposition = root.CreateElement("WorldPosition");
                SetAttribute("x", "300.0", worldposition);
                SetAttribute("y", "300.0", worldposition);
                SetAttribute("z", "0.3", worldposition);
                SetAttribute("h", "90", worldposition);
                entityCondition.AppendChild(reachPositionCondition);
                reachPositionCondition.AppendChild(position);
                position.AppendChild(worldposition);
            }
            else
            {
                Console.WriteLine("Naming error in Entity condition. This name is not supported.");
            }
        }
        */

        /// helper
        private void SetAttribute(string name, string value, XmlNode element)
        {
            XmlAttribute attribute = root.CreateAttribute(name);
            attribute.Value = value;
            element.Attributes.Append(attribute);
        }

    }
}

public static class Helper
{
    public static string? FirstCharToLowerCase(this string? str)
    {
        if (!string.IsNullOrEmpty(str) && char.IsUpper(str[0]))
            return str.Length == 1 ? char.ToLower(str[0]).ToString() : char.ToLower(str[0]) + str[1..];

        return str;
    }
}


/* All Value Conditions
            <!-- parameterCondition -->
            <!-- timeOfDayCondition -->
            <!-- IMPLEMENTED simulationTimeCondition params: value(float), rule (enum(less, greater, equal))-->
            <!-- IMPLEMENTED storyboardElementStateCondition params: storyboardElementType(enum(6 options)), storyboardElementRef(string), state (enum(7 options))-->
            <!-- userDefinedValueCondition -->
            <!-- trafficSignalCondition -->
            <!-- trafficSignalControllerCondition -->
            */

/* All EntityConditions
            <!-- endOfRoadCondition-->
            <!-- collisionCondition-->
            <!-- offroadCondition-->
            <!-- timeHeadwayCondition-->
            <!-- timeToCollisionCondition-->
            <!-- accelerationCondition-->
            <!-- standStillCondition-->
            <!-- speedCondition-->
            <!-- relativeSpeedCondition-->
            <!-- traveledDistanceCondition params-->
            <!-- IMPLEMETED reachPositionCondition params: tolerance(float), WorldPosition (WorldPosition CREATE STRING FROM X,Y,Z,H)  -->
            <!-- distanceCondition params-->
            <!-- relativeDistanceCondition-->
            */