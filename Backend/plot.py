import matplotlib.pyplot as plt
from matplotlib.ticker import MultipleLocator

# import numpy as np
# data = {
#     "x": np.random.randn(24),
#     "xLabels": [f"{i:02d}:00" for i in range(24)],
# }

import json
with open("data.json") as f:
    data = json.load(f)

fig, ax = plt.subplots(figsize=(6,4))

# plot data
x = data["x"]
plt.plot(range(len(x)), x)

# set x tick labels
labels = data["xLabels"]
# take every 3rd label
reduced_labels = [label if i % 3 == 0 else "" for i, label in enumerate(labels)]
plt.xticks(range(len(labels)), reduced_labels)
ax.xaxis.set_major_locator(MultipleLocator(3))
ax.xaxis.set_minor_locator(MultipleLocator(1))
plt.xlim(0, len(labels)-1)

# set axis limits
ymin = min(0.5, min(x))
ymax = max(1.5, max(x))
plt.ylim(ymin, ymax)

# set axis labels
plt.xlabel("Uhrzeit")
plt.ylabel("Musikvielfalt")

plt.title("Musikvielfalt per Stunde")
plt.grid(which="major")
plt.grid(which="minor")
plt.savefig("plot.png")