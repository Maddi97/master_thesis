from matplotlib.ticker import FuncFormatter
import pandas as pd
import seaborn as sns
import matplotlib.pyplot as plt
import os
plt.rcParams.update({'font.size': 16})
plt.figure(figsize=(12, 9))


def millions_formatter(x, pos):
    return f'{x/1e6}M'


formatter = FuncFormatter(millions_formatter)

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
df_steps = pd.DataFrame()

dfs = [] 
N = 10  # Aggregate


for index, file in enumerate(csv_files):
    data = pd.read_csv(file)

    data = data.reindex(data.index.repeat(data['steps']))

    data['step'] = data.groupby(level=0).cumcount()
    data.reset_index(drop=True, inplace=True)

    data = data.groupby(data.index // N).mean()

    data = data.drop(['episodeNr', 'steps'], axis=1)

    data['Experiment'] = experiment_names[index]

    dfs.append(data)

# Concatenate all dataframes at once
df_steps = pd.concat(dfs, ignore_index=True)

print('Generated Df')

plt.grid(True)
for index, d in enumerate(dfs):
    print(f"Approach Nr. {index}")
    sns.lineplot(x=d.index * N, y=d['velocity'],
                 label=experiment_names[index], color=colors[index])


plt.title('Average Velocity Development in Training')
plt.gca().xaxis.set_major_formatter(formatter)

plt.xlabel('Steps')
plt.ylabel('Velocity in unit/s')

plt.savefig('images_training/velocity_development.png')

plt.cla()

print('Plotted Velocity')
plt.grid(True)
for index, d in enumerate(dfs):
    print(f"Approach Nr. {index}")
    sns.lineplot(x=d.index * N, y=d['time'],
                 label=experiment_names[index], color=colors[index])

plt.title('Time Duration Development in Training')
plt.gca().xaxis.set_major_formatter(formatter)

plt.xlabel('Steps')
plt.ylabel('Time in Seconds')

plt.savefig('images_training/time_development.png')


plt.cla()
print('Plotted Time')

plt.grid(True)
for index, d in enumerate(dfs):
    print(f"Approach Nr. {index}")
    sns.lineplot(x=d.index * N, y=d['reward'],
                 label=experiment_names[index], color=colors[index])


plt.title('Cumulative Reward Development in Training')
plt.gca().xaxis.set_major_formatter(formatter)

plt.xlabel('Steps')
plt.ylabel('Reward')

plt.savefig('images_training/reward_development.png')
