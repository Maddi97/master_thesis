# This is a sample Python script.

# Press ⌃R to execute it or replace it with your code.
# Press Double ⇧ to search everywhere for classes, files, tool windows, actions, and settings.
import csv

import NeuralNetwork
import Communicator
import Manager
import numpy as np
import time


def sortfunction(neuralnet: NeuralNetwork.NeuralNet):
    return neuralnet.getperformance()


# Press the green button in the gutter to run the script.
if __name__ == '__main__':
    neuralnet = [[4, 3, 4], [4, 3, 4], [4, 3, 4], [12, 4, 3,2]]
    #neuralnet = [[4, 3, 4], [4, 3, 4], [4, 3, 4], [12,4, 2]]
    neuralneta = []
    botsize = 30
    port = 9653
    # mana = Manager.CommunicationManager(port, "127.0.0.1", botsize)
    # port += 4
    # time.sleep(0.5)
    # mana.setshuffle()
    # mana.setstart()
    for x in range(botsize):
        neuro = NeuralNetwork.NeuralNet(neuralnet, upperbound=1, lowerbound=-1)
        neuro.initvalues()
        # print(neuro.getencoded())
        # print(neuro.createoppositeindividual())
        neuralneta.append(neuro)
    # for i in neuralneta:
    #     i.setperfromance(np.random.random())
    #     print(i.getencoded())
    # neuralneta.sort(key=sortfunction,reverse=True)
    # for i in neuralneta:
    #    print(i.getencoded())
    # t = neuralneta[0].createoppositeindividual(1,-1)
    # print(len(t.getencoded()))
    cendobl = NeuralNetwork.CENDEDOBL(neuralneta, 0.3, 30, neuralnet, port, 1,
                                      save="./data")
    cendobl.starmanger()
    #cendobl.readindata("./data/30_104.json")
    # res = cendobl.benchmark(3)
    # with open('sample30.csv', 'w') as f:
    #     mywriter = csv.writer(f, delimiter=',')
    #     mywriter.writerows(res)
    #cendobl.CenDEDOL(0.9, 0.5, 3)
    cendobl.DE(0.9, 0.5, newrun=True)
    #cendobl.readindata("./30popsize/57.json")
    #t = cendobl.getpop()
    #mana = Manager.CommunicationManager(port, "127.0.0.1", 1)
    #port += 4
    #mana.setshuffle()
    #mana.setstart()
    #commm = Communicator.Communicator(port, "127.0.0.1", t[0])
    # time.sleep(30)
    # mana.setstop()
    # commm.setstoprunning()
    # commm.stopthread()

    # cendobl.CenDEDOL(0.9, 0.5, 3)
    # time.sleep(30)
# print("stopping")
# mana.setstop()
# for x in neuralneta:
#   x.setstoprunning()
# for x in neuralneta:
#    x.stopthread()
# results = mana.getresults()
# while results is None:
#    results = mana.getresults()
# print(results)
# time.sleep(1)
# mana.setstop()
# print("hallo")
# comm = Communicator.Communicator(8653, "127.0.0.1", nn)
# time.sleep(40)
# del comm
# input_data = np.random.rand(3,5,4).tolist()
# start = time.time()
# out =neuralneta[0].neuralnet(input_data)
# end = time.time()
# print("The time of execution of above program is :", end - start)
# input_data = np.random.rand(3, 5, 4).tolist()
# start = time.time()
# out = neuralneta[0].neuralnetwithoutthread(input_data)
# end = time.time()
# print(out)
# print("The time of execution of above program is :", end - start)


# See PyCharm help at https://www.jetbrains.com/help/pycharm/
