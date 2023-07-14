import numpy as np
import pandas as pd
import seaborn as sns
import matplotlib.pyplot as plt
import os
from matplotlib.ticker import FuncFormatter
from statistics import mean
plt.rcParams.update({'font.size': 16})
plt.figure(figsize=(12, 9))

EXPERIMENT = 1
IMAGE_PATH = f'images_experiments/experiment_{EXPERIMENT}' 
APPROACHES = ['PPO-MEM-FMT', 'PPO-MEM-SGT',
                    'PPO-BASIC-SGT', 'PPO-BASIC-FMT']
COLORS = ['#E52535', '#F97E00', '#005DAF', '#42AEB3']
MAPS = ['easyGoalLaneMiddleBlueFirst', 'easyGoalLaneMiddleRedFirst', 'twoGoalLanesBlueFirstLeftMedium', 'twoGoalLanesBlueFirstRightMedium', 'twoGoalLanesRedFirstLeftMedium',
        'twoGoalLanesRedFirstRightMedium', 'twoGoalLanesBlueFirstLeftHard', 'twoGoalLanesBlueFirstRightHard',  'twoGoalLanesRedFirstLeftHard', 'twoGoalLanesRedFirstRightHard']

MAP_STRINGS = ['easy blue', 'easy red', 'medium blue left', 'medium blue right', 'medium red left',
                      'medium red right', 'hard blue left', 'hard blue right',  'hard red left', 'hard red right']
MAP_DIFF = ['Easy', 'Medium', 'Hard'] if EXPERIMENT == 1 else ['Medium']

def get_average_map_difficulty(l):
    if(np.nan in l):
        l = [x for x in l if str(x) != 'nan']

        return [mean([l[0], l[1]]), mean(l[2:6]), mean(l[6:])]
    else:
        return [mean([l[0], l[1]]), mean(l[2:6]), mean(l[6:10])]
    
        

def get_csv_files():
    results_path = f'results_experiments/experiment_{EXPERIMENT}'

    csv_files = [os.path.join(results_path, f'{approach.lower().replace("-", "_")}_experiment_{EXPERIMENT}_f.csv') for approach in APPROACHES ]
    return csv_files


def plot_compare_success_ratio(csv_files):
    plt.grid(True, axis='y')

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

        counts_diff.append(get_average_map_difficulty(counts))

    width = 0.15
    x = np.arange(len(MAP_DIFF))
    total_width = len(csv_files) * width
    offset = total_width / 2.8 # until its in the center

    for index, counts_diff_values in enumerate(counts_diff):
        plt.bar(x=x+index*width-offset, height=counts_diff_values,
                color=COLORS[index], width=width, label=f'{APPROACHES[index]}')
        for i, value in enumerate(counts_diff_values):
            plt.text(i + index * width - offset, value + 1,
                     str(value), ha='center', va='bottom')


    plt.title('Success Ratio for Different Map Difficulties')
    plt.xticks(x, MAP_DIFF)
    plt.xlabel('Map difficulty')
    plt.ylabel('Success ratio')
    if EXPERIMENT is not 1:
        plt.yticks(np.arange(0, 111, 20))
    plt.legend()
    plt.savefig(os.path.join(
        IMAGE_PATH, f'success_rate_per_map_experiment_{EXPERIMENT}'))
    plt.cla()

def plot_compare_average_time(csv_files):
    plt.grid(True, axis='y')

    times_diff = []
    for index, csv_file in enumerate(csv_files):
        df = pd.read_csv(csv_file)
        times = []

        for map in MAPS:
            filtered_df = df[df['map'] == map]
            filtered_df = filtered_df[filtered_df['endEvent'] == 'completeMap']
            times.append(filtered_df['time'].mean())
        times_diff.append(get_average_map_difficulty(times))

    width = 0.15
    x = np.arange(len(MAP_DIFF))
    total_width = len(csv_files) * width
    offset = total_width / 2.8 # until its in the center

    for index, times_diff_values in enumerate(times_diff):
        plt.bar(x=x+index*width-offset, height=times_diff_values,
                color=COLORS[index], width=width, label=f'{APPROACHES[index]}')
        for i, value in enumerate(times_diff_values):
            plt.text(i + index * width - offset, value + 0.2,
                     f'{value:.1f}', ha='center', va='bottom')

    plt.yticks(np.arange(0, 16.1, 2))

    plt.title('Average Completion Time for every Difficulty')
    plt.xticks(x, MAP_DIFF)
    plt.xlabel('Map Difficulty')
    plt.ylabel('Completion Time in Seconds')

    plt.legend()
    plt.savefig(os.path.join(
        IMAGE_PATH, f'time_per_map_experiment_{EXPERIMENT}'))



def plot_compare_average_velocity(csv_files):
    plt.figure(figsize=(10, 8))
    plt.grid(True)

    velocities_diff = []

    for index, csv_file in enumerate(csv_files):
        df = pd.read_csv(csv_file)
        velocities = []
        for map in MAPS:
            filtered_df = df[df['map'] == map]
            velocities.append(filtered_df['velocity'].mean())
        velocities_diff.append(get_average_map_difficulty(velocities))

    width = 0.15
    x = np.arange(len(MAP_DIFF))
    total_width = len(csv_files) * width
    offset = total_width / 2.8 # until its in the center

    for index, velocities_diff_values in enumerate(velocities_diff):
        plt.bar(x=x+index*width-offset, height=velocities_diff_values,
                color=COLORS[index], width=width, label=f'{APPROACHES[index]}')

    plt.title('Success rate in each map for different approaches')
    plt.xticks(x, MAP_DIFF)
    plt.xlabel('Map difficulty')
    plt.ylabel('Average velocity of one map')
    plt.legend()
    plt.savefig(os.path.join(
        IMAGE_PATH, f'velocities_per_map_experiment_{EXPERIMENT}'))



csv_files=get_csv_files()
plot_compare_success_ratio(csv_files=csv_files)
plot_compare_average_time(csv_files=csv_files)
