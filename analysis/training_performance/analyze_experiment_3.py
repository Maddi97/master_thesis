import numpy as np
import pandas as pd
import seaborn as sns
import matplotlib.pyplot as plt
import os
from matplotlib.ticker import FuncFormatter
from statistics import mean
plt.rcParams.update({'font.size': 16})
plt.figure(figsize=(12, 9))
EXPERIMENT = 3
IMAGE_PATH = f'images_experiments/experiment_{EXPERIMENT}'
APPROACHES = ['PPO-MEM-FMT', 'PPO-MEM-SGT',
              'PPO-BASIC-SGT', 'PPO-BASIC-FMT']
COLORS = ['#E52535', '#F97E00', '#005DAF', '#42AEB3']
MAPS = ['easyGoalLaneMiddleBlueFirst', 'easyGoalLaneMiddleRedFirst', 'twoGoalLanesBlueFirstLeftMedium', 'twoGoalLanesBlueFirstRightMedium', 'twoGoalLanesRedFirstLeftMedium',
        'twoGoalLanesRedFirstRightMedium', 'twoGoalLanesBlueFirstLeftHard', 'twoGoalLanesBlueFirstRightHard',  'twoGoalLanesRedFirstLeftHard', 'twoGoalLanesRedFirstRightHard']

MAP_STRINGS = ['easy blue', 'easy red', 'medium blue left', 'medium blue right', 'medium red left',
               'medium red right', 'hard blue left', 'hard blue right',  'hard red left', 'hard red right']
MAP_DIFF = ['Medium']


def get_average_map_difficulty(l):
    return [mean([l[0], l[1]]), mean(l[2:6]), mean(l[6:10])]


def get_csv_files(engine_str):
    results_path = f'results_experiments/experiment_{EXPERIMENT}'

    csv_files = [os.path.join(
        results_path, f'{approach.lower().replace("-", "_")}_experiment_{EXPERIMENT}{engine_str}_f.csv') for approach in APPROACHES]
    return csv_files


def plot_compare_success_ratio(csv_files, engine_str):


    plt.grid(True, axis='y')  # Add this line to your code

    counts_diff = []

    for index, csv_file in enumerate(csv_files):
        df = pd.read_csv(csv_file)
        counts = []
        for map in MAPS:
            filtered_df = df[df['map'] == map]
            if 'completeMap' in filtered_df['endEvent'].values:
                count = filtered_df['endEvent'].value_counts()['completeMap']
            else:
                count = 0
            counts.append(count)

        counts_diff.append(get_average_map_difficulty(counts)[1])

    plt.bar(x=APPROACHES, height=counts_diff,
            color=COLORS)

    for i, value in enumerate(counts_diff):
        plt.text(i, value, str(value), ha='center', va='bottom')
    plt.title(f'Success Ratio with {engine_str}% Motor Power')
    plt.xlabel('Algorithm')
    plt.ylabel('Success ratio')
    if EXPERIMENT is not 1:
        plt.yticks(np.arange(0, 111, 20))
    plt.savefig(os.path.join(
        IMAGE_PATH, f'success_rate_per_map_experiment_{EXPERIMENT}_{engine_str}'))
    plt.cla()


def plot_compare_average_time(csv_files, engine_str):


    plt.grid(True, axis='y')

    times_diff = []
    for index, csv_file in enumerate(csv_files):
            df = pd.read_csv(csv_file)
            times = []
            for map in MAPS:
                filtered_df = df[df['map'] == map]
            # all complete map events
                filtered_df = filtered_df[filtered_df['endEvent'] == 'completeMap']

                times.append(filtered_df['time'].mean())

            times_diff.append(get_average_map_difficulty(times)[1])

    plt.bar(x=APPROACHES, height=times_diff,
            color=COLORS)
    for i, value in enumerate(times_diff):
        plt.text(i, value, f'{value:.1f}s', ha='center', va='bottom')
    plt.title(f'Average Completion Time for every Difficulty with {engine_str}% motor power')
    plt.xlabel('Algorithm')
    plt.ylabel('Completion Time in Seconds')

    plt.savefig(os.path.join(
        IMAGE_PATH, f'time_per_map_experiment_{EXPERIMENT}_{engine_str}'))
    plt.cla()


for engine_str in ['-20', '+20']:
    csv_files = get_csv_files(engine_str)
    plot_compare_success_ratio(csv_files=csv_files, engine_str=engine_str)
    plot_compare_average_time(csv_files=csv_files, engine_str=engine_str)
