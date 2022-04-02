import matplotlib.pyplot as plt
from matplotlib.ticker import MultipleLocator

do_bar_plot = False

# import numpy as np
# data = {
#     "x": np.random.randn(24),
#     "xLabels": [f"{i:02d}:00" for i in range(24)],
#     "stds": None,
#     "title": None,
#     "height": 6,
#     "width": 4,
# }

import json
with open("data.json") as f:
    data = json.load(f)

fig, ax = plt.subplots(figsize=(data["width"], data["height"]))

# plot data
x = data["x"]
x_min = data["x_min"]
x_max = data["x_max"]
if do_bar_plot:
    plt.bar(range(len(x)), x, zorder=3, yerr=x - x_min)
else:
    plt.plot(range(len(x)), x, zorder=3)
    if x_min is not None and x_max is not None:
        plt.fill_between(range(len(x)), x_min, x_max, alpha=0.3, zorder=3)

# set x tick labels
labels = data["xLabels"]
# take every 3rd label
reduced_labels = [label if i % 3 == 0 else "" for i, label in enumerate(labels)]
plt.xticks(range(len(labels)), reduced_labels)
ax.xaxis.set_major_locator(MultipleLocator(3))
ax.xaxis.set_minor_locator(MultipleLocator(1))
plt.xlim(-0.5, len(labels)-0.5)

# set axis limits
plt.ylim(0, 100.05)

# set axis labels
plt.xlabel("Uhrzeit")
plt.ylabel("Musikvielfalt")

if data["title"] is None:
    plt.title("Musikvielfalt per Stunde")
else:
    plt.title(data["title"])

if do_bar_plot:
    plt.grid(axis="y", color="white")
else:
    plt.grid(which="major", color="white")
    plt.grid(which="minor", color="white")
ax.set_facecolor('whitesmoke')
plt.savefig("plot.png")