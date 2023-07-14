import pandas as pd
import seaborn as sns
import matplotlib.pyplot as plt
import os
from matplotlib.ticker import FuncFormatter
plt.rcParams.update({'font.size': 16})
plt.figure(figsize=(12, 9))

def millions_formatter(x, pos):
    return f'{x/1e6}M'

formatter = FuncFormatter(millions_formatter)
results_path = 'results_training'


def plot_compare_cumulative_reward(experiment_names, colors):


    csv_files = [os.path.join(
        results_path, f'{exp_name}-reward.csv') for exp_name in experiment_names]
    # Adjust the width and height as per your requirement
    plt.figure(figsize=(9, 7))

    for plot in [0,1]:
        for index, file in enumerate(csv_files):
            data = pd.read_csv(file)
            experiment_name = experiment_names[index]
            data['Smoothed Value'] = data['Value'].rolling(
                window=int(len(data) * 0.1), min_periods=1).mean()


            if plot==0:
                sns.lineplot(data=data, x='Step', y='Value',
                        color=colors[index], alpha=0.2, legend=False)

            if plot==1:
                sns.lineplot(data=data, x='Step', y='Smoothed Value',
                        color=colors[index], label=experiment_name)

    plt.grid(True)

    plt.gca().xaxis.set_major_formatter(formatter)
    plt.legend()
    plt.title(
        f'Cumulative Reward of {experiment_names[0]} and {experiment_names[1]}')
    plt.xlabel('Steps')
    plt.ylabel('Cumulative Reward')
    plt.savefig(
        f'images_training/cum_reward_{experiment_names[0]}_{experiment_names[1]}.png')


def plot_compare_cumulative_reward_normalized(experiment_names, colors):

    csv_files = [os.path.join(
        results_path, f'{exp_name}-reward.csv') for exp_name in experiment_names]
    # Adjust the width and height as per your requirement
    plt.figure(figsize=(9, 7))

    max_value = 0
    for plot in [0, 1]:

        for index, file in enumerate(csv_files):
            data = pd.read_csv(file)
            experiment_name = experiment_names[index]
            data['Smoothed Value'] = data['Value'].rolling(
                window=int(len(data) * 0.1), min_periods=1).mean()

            # Normalize the values
            normalized_value = (data['Value'] - data['Value'].min()) / \
                (data['Value'].max() - data['Value'].min())
            normalized_smoothed_value = (data['Smoothed Value'] - data['Smoothed Value'].min()) / (
                data['Smoothed Value'].max() - data['Smoothed Value'].min())

            max_value = max(max_value, normalized_value.max(),
                            normalized_smoothed_value.max())
            if plot == 2:
                sns.lineplot(data=data, x='Step', y=normalized_value,
                        color=colors[index], alpha=0.2, legend=False)
            elif plot == 0:
                sns.lineplot(data=data, x='Step', y=normalized_smoothed_value,
                        color=colors[index], label=experiment_name)

    plt.grid(True)

    plt.ylim(0, max_value)  # Set the same y-axis limits for both experiments

    plt.gca().xaxis.set_major_formatter(formatter)
    plt.legend()
    plt.title(f'Normalized Cumulative Reward Comparison')
    plt.xlabel('Steps')
    plt.ylabel('Normalized Cumulative Reward')
    plt.savefig(f'images_training/cumulative_reward_comparison_only_smoothed.png')


experiment_names = ['PPO-MEM-FMT', 'PPO-BASIC-FMT']
colors = ['#E52535', '#42AEB3']
plot_compare_cumulative_reward(
    experiment_names=experiment_names, colors=colors)


experiment_names = ['PPO-MEM-SGT',
                    'PPO-BASIC-SGT']
colors = ['#F97E00', '#005DAF']
plot_compare_cumulative_reward(
    experiment_names=experiment_names, colors=colors)

experiment_names = ['PPO-MEM-FMT', 'PPO-MEM-SGT',
                    'PPO-BASIC-SGT', 'PPO-BASIC-FMT']
colors = ['#E52535', '#F97E00', '#005DAF', '#42AEB3']
plot_compare_cumulative_reward_normalized(experiment_names=experiment_names, colors=colors)
