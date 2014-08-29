#!/usr/bin/env python

# this script takes a mask on the alpha channel of an RGBA image and extracts the corresponding scanlines in c# to be copy-pasted into VPL Viewer for Unity

import matplotlib.pyplot as plt
import matplotlib.image as mpimg
import numpy as np
import sys

if __name__ == "__main__":
	# load image
	img = mpimg.imread(sys.argv[1])
	# scanlines generator
	def scanlines():
		for y in range(img.shape[0]):
			start = None
			end = None
			for x in range (img.shape[1]):
				if img[y,x,3] != 0.:
					if start == None:
						start = x
					end = x
			if start != None:
				# unity handles y-coordinate for texture the reverse of what is use for images
				yield (img.shape[0]-1-y, start, end)
	
	# print scanlines
	print "new Scanlines { " + str(", ").join(map(lambda t: "{{ {0}, {1}, {2} }}".format(*t), reversed(list(scanlines())))) + " }"