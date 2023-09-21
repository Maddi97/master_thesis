# Train an Agent with Reinforcement Learning Using Visual Camera Input

## Master Thesis - Maximilian Schaller Matr.-Nr. 3750663

### How to launch the Project

1. Clone this Repository
2. Unity must be installed (Unity Hub + Unity Editor - Version 2021.3.1f1)
3. In Unity open the carsim folder of this project

- carsim/ includes all necessary scripts and code related to the simulation and RL
- analysis/ includes all scripts for the visual analysis and chart generation

4. In Unity go to `file > open scene > TrainArenaCollection` to open the scene.
5. Scads (Thomas B.) has explanation Videos, whatch them to know about the different training configuration.
6. Check the Logging and result path. They must fit to your system.
7. A common problem is that the used library EMGU.CV downloaded from the Nuget store has some errors with the delivered DLLs. Thus, the error of the library has to be fixed by hand, by installing it from source and reference the correct libraries. This is required to use the image pre-processing pipeline.
8. To use the machine learning (ML-Agents) the ML-Agents Framework needs to be installed. [ML-Agents Installation Guide](https://github.com/Unity-Technologies/ml-agents/blob/develop/docs/Installation.md)
