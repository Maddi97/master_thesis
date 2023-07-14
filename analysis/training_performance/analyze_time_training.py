import pandas as pd
import seaborn as sns
import matplotlib.pyplot as plt
import os
from matplotlib.ticker import FuncFormatter
plt.rcParams.update({'font.size': 16})
plt.figure(figsize=(12, 9))

results_path = 'results_training'
csv_files = [
    os.path.join(results_path, 'training_results_run_0.csv'),
    os.path.join(results_path, 'training_results_run_1.csv'),
    os.path.join(results_path, 'training_results_run_2.csv'),
    os.path.join(results_path, 'training_results_run_3.csv')
]

experiment_names = ['PPO-MEM-FMT', 'PPO-MEM-SGT',
                    'PPO-BASIC-SGT', 'PPO-BASIC-FMT']
colors = ['#E52535', '#F97E00', '#005DAF', '#42AEB3']
time_sums= []
for index, file in enumerate(csv_files):
    data = pd.read_csv(file)
    time_sum = data['time'].sum()
    time_sums.append(time_sum/3600)



# Create the formatter

plt.grid(True, axis='y')

plt.bar(x=experiment_names, height=time_sums, color=colors)
plt.yticks(range(0, int(max(time_sums)+5), 2))

plt.title('Total Time for Each Training')
plt.xlabel('Algorithm')
plt.ylabel('Time in Hours')
plt.savefig('images_training/total_time_barplot.png')
