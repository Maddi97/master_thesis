import pandas as pd
import matplotlib.pyplot as plt
import os
plt.rcParams.update({'font.size': 16})
plt.figure(figsize=(12, 9))
APP = 0
EXPERIMENT = 1
IMAGE_PATH = f'images_experiments/experiment_failure'
APPROACHES = ['PPO-MEM-FMT', 'PPO-MEM-SGT',
              'PPO-BASIC-SGT', 'PPO-BASIC-FMT']
COLORS = ['#E52535', '#F97E00', '#005DAF', '#42AEB3']

MAP_STRINGS= ['easy blue', 'easy red', 'medium blue left', 'medium blue right', 'medium red left',
'medium red right', 'hard blue left', 'hard blue right',  'hard red left', 'hard red right']
def get_csv_files():
    results_path = f'results_experiments/experiment_{EXPERIMENT}'

    csv_files = [os.path.join(
        results_path, f'{approach.lower().replace("-", "_")}_experiment_{EXPERIMENT}_f.csv') for approach in APPROACHES]
    return csv_files


def create_map_barplot(csv_file):
    # Read the CSV file into a pandas DataFrame
    data = pd.read_csv(csv_file)

    # Filter the DataFrame for the specified map types and endEvent as 'completeMap'
    map_types = ['twoGoalLanesBlueFirstLeftHard', 'twoGoalLanesBlueFirstRightHard',
                 'twoGoalLanesRedFirstLeftHard', 'twoGoalLanesRedFirstRightHard']
    map_strings = ['hard blue left', 'hard blue right',
                   'hard red left', 'hard red right']
    count = []
    for map in map_types:
        filtered_df = data[data['map'] == map]
        filtered_df = filtered_df[filtered_df['endEvent'] ==
                    'completeMap']  # all complete map events
        count.append(len(filtered_df.index))

    # Count the number of rows for each map type
    #map_counts = filtered_data['map'].value_counts()

    # Create the bar plot
    plt.bar(map_strings, count, color=COLORS[APP])
    for i, value in enumerate(count):
        plt.text(i, value + 0.2,
                 f'{value:.1f}', ha='center', va='bottom')

    # Set plot labels and title
    plt.xlabel('Map')
    plt.ylabel('Success ratio')
    plt.title(f'Success Ratio of the {APPROACHES[APP]} algorithm\nin all configurations of the hard map')

    # Rotate x-axis labels if needed
    #plt.xticks(rotation=45)

    # Display the plot
    plt.savefig(f'{IMAGE_PATH}/Failure_{APPROACHES[APP]}_per_map.png')
    plt.cla()


for i in range(4):
    APP = i
    create_map_barplot(get_csv_files()[APP])
