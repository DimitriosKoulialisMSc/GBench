using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;


namespace Assets.Scripts
{
    // This class will run all our scoring for our tests.
    public class Evaluator 
    {
        #region Variables
        private static readonly object _withThisObject = new object();
        private static Evaluator _evaluator;
        public decimal updateInterval = 0.5M;
        private decimal lastInterval;
        private int frames = 0;
        private decimal relativeDifference;
        private Dictionary<int, decimal> cpuReferences, avgFramesPerStages;
        private static decimal _physicsScore;
        public static decimal physicsScore
        {
            get
            {
                return _physicsScore;
            }
        }
        private static decimal _graphicsScore;
        public static decimal graphicsScore
        {
            get
            {
                return _graphicsScore;
            }
        }

        private static decimal _totalScore;
        public static decimal totalScore
        {
            get
            {
                return _totalScore;
            }
            set
            {
                _totalScore = value;
            }
        }

        private static decimal _averageFramesForFirstTest;
        private static decimal _exposedAvgFrames;
        public static decimal exposedAvgFrames
        {
            get
            {
                return _exposedAvgFrames;
            }
            
        }

        private static DateTime lastTime;
        private static TimeSpan lastTotalProcessorTime;
        private static DateTime curTime;
        private static TimeSpan curTotalProcessorTime;

        private PerformanceCounter processCpuUsage;

        private static decimal weightForGraphics, weightedGraphicsScore, weightForPhysics, weightedPhysicsScore;
        #endregion

        #region Constructors
        public static Evaluator evaluator
        {
            get
            {
                lock (_withThisObject)
                {   
                   if(_evaluator == null)
                    {
                        _evaluator = new Evaluator();
                    }
                    return _evaluator;
                } 
            }
        }

        public Evaluator()
        {
            lastInterval = (decimal)Time.realtimeSinceStartup;
            cpuReferences = new Dictionary<int, decimal>();
            avgFramesPerStages = new Dictionary<int, decimal>();
            //processCpuUsage = new PerformanceCounter(Process.GetCurrentProcess().ProcessName, "% Processor Time", "_Total");
            frames = 0;
        }
        #endregion

        #region Data Accumulation & Calculations
        public void UpdateCumulativeMovingAverageFPS()
        {
            ++frames;
            decimal timeNow = (decimal)Time.realtimeSinceStartup;
            if (timeNow > lastInterval + updateInterval)
            {
                _exposedAvgFrames = (frames / (timeNow - lastInterval));
                frames = 0;
                lastInterval = timeNow;
            }
            
        }      
        public float GetCpuUsage()
        {
            string processName = "Unity";
            double CPUUsage = 0d;

            Process[] proccessArray = Process.GetProcessesByName(processName);
            if (proccessArray.Length == 0)
            {
                UnityEngine.Debug.Log(processName + " does not exist");
            }
            else
            {
                Process focusedProcess = proccessArray[0];
                if (lastTime == null || lastTime == new DateTime())
                {
                    lastTime = DateTime.Now;
                    lastTotalProcessorTime = focusedProcess.TotalProcessorTime;
                }
                else
                {
                    curTime = DateTime.Now;
                    curTotalProcessorTime = focusedProcess.TotalProcessorTime;

                    CPUUsage = (curTotalProcessorTime.TotalMilliseconds - lastTotalProcessorTime.TotalMilliseconds) / curTime.Subtract(lastTime).TotalMilliseconds / Convert.ToDouble(Environment.ProcessorCount);
                    

                    lastTime = curTime;
                    lastTotalProcessorTime = curTotalProcessorTime;
                }
            }
            return (float)(CPUUsage * 100);
        }

        public void CalculateGraphicsScore()
        {
            if(_exposedAvgFrames!=0 && _averageFramesForFirstTest == 0)
            {
                _averageFramesForFirstTest = _exposedAvgFrames;

            }
            else
            {

                _graphicsScore = Convert.ToDecimal(230) * (Convert.ToDecimal(2) / ((Convert.ToDecimal(1) / _averageFramesForFirstTest) + (Convert.ToDecimal(1) / _exposedAvgFrames))); 
            }
        }

        public void StageAverageFrames(int stageFromStressor)
        {
            avgFramesPerStages.Add(stageFromStressor, _exposedAvgFrames);
            _exposedAvgFrames = 0;
        }

        public void CalculatePhysicsScore()
        {
            GoThroughAccumulatedData();
        }

        private void GoThroughAccumulatedData()
        {
            foreach (KeyValuePair<int, decimal> cpuKeyValue in cpuReferences)
            {
                if (cpuKeyValue.Key == 1)
                {
                    _physicsScore += Math.Abs(avgFramesPerStages[cpuKeyValue.Key]);

                }
                else
                {
                    relativeDifference = CalculateRelativeDifferenceOfFramesOverCPUusage(cpuKeyValue.Key);
                    _physicsScore += relativeDifference * Math.Abs(avgFramesPerStages[cpuKeyValue.Key]);

                }
            }
            _physicsScore *= 9;
        }

       
        private decimal CalculateRelativeDifferenceOfFramesOverCPUusage(int inputedKey)
        {
            UnityEngine.Debug.Log("-fps" + avgFramesPerStages[inputedKey - 1]);
            UnityEngine.Debug.Log(".fps" + avgFramesPerStages[inputedKey]);
            UnityEngine.Debug.Log("/cpu" + cpuReferences[inputedKey]);
            return (avgFramesPerStages[inputedKey-1] - avgFramesPerStages[inputedKey]) / cpuReferences[inputedKey];
        }

        public void AccumulateDataForPhysicsScoreCalculation(int stageFromStressor, float currentCPUusage)
        {
            cpuReferences.Add(stageFromStressor, (decimal)currentCPUusage);
        }

        public void CalculateFinalScore()
        {
            weightForGraphics = Convert.ToDecimal(7) / Convert.ToDecimal(9);
            weightedGraphicsScore = Evaluator._graphicsScore * weightForGraphics;
            weightForPhysics = Convert.ToDecimal(2) / Convert.ToDecimal(9);
            weightedPhysicsScore = Evaluator._physicsScore * weightForPhysics;
            _totalScore = ((weightedGraphicsScore + weightedPhysicsScore) / (weightedGraphicsScore / Evaluator._graphicsScore) + (weightedPhysicsScore / Evaluator._physicsScore))/Convert.ToDecimal(2);
        }

        #endregion

        #region Printers   
        public void showMeAllGraphicsValues()
        {
            foreach (KeyValuePair<int, decimal> cpuKeyValue in avgFramesPerStages)
            {
                UnityEngine.Debug.Log("GraphicsKey: " + cpuKeyValue.Key.ToString() + " GraphicsValue: " + Math.Abs(cpuKeyValue.Value).ToString());
            }
        }

        public void showMeAllCPUReferences()
        {
            foreach (KeyValuePair<int, decimal> cpuKeyValue in cpuReferences)
            {
                UnityEngine.Debug.Log("Key: " + cpuKeyValue.Key.ToString() + "Value: " + Math.Abs(cpuKeyValue.Value).ToString());
            }
        }

        #endregion
        
        public void MyDestructor()
        {
            _evaluator = null;
        }

    }
}
