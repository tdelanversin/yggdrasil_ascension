import os
import shutil
import PIL.Image as Img
import numpy as np
import scipy.signal as scs
import scipy.ndimage as scn

def makeGaussian(size, fwhm = 3, center=None):
    """ Make a square gaussian kernel.

    size is the length of a side of the square
    fwhm is full-width-half-maximum, which
    can be thought of as an effective radius.
    """

    x = np.arange(0, size, 1, float)
    y = x[:,np.newaxis]

    if center is None:
        x0 = y0 = size // 2
    else:
        x0 = center[0]
        y0 = center[1]

    return np.exp(-4*np.log(2) * ((x-x0)**2 + (y-y0)**2) / fwhm**2)

h_offset = 16
count = 8
width = 256
height = 128
a = 1.0
b = 1.0
new_img = np.zeros((height+2*h_offset, width*count, 4), dtype = np.byte)
# new_img[0:img.shape[0],0:img.shape[1],:] = img

gauss = makeGaussian(17, 3)

import math

exp = 1.0
dexp = 0.5
for c in range(0, count):
    if(c == 4):
        dexp = -dexp
    lspx = np.linspace(-1, 1, height)
    lspy = np.linspace(-1, 1, width)
    mesh = np.meshgrid(lspy, lspx)

    # n = np.random.random((height, width))
    # n[n > 0.01] = 0
    # n[n > 0] = 1

    # row = np.ones(17)
    # masky = np.tile(row, (len(row),1))
    # masky = ((masky * (np.ones(len(row))*np.arange(0,len(row)))).T - math.floor(len(row)/2)).astype(np.int32)
    # maskx = masky.T
    # for i in range(int(len(row)/2),height-int(len(row)/2)):
    #     for j in range(int(len(row)/2), width-int(len(row)/2)):
    #         if n[i,j] == 1:
    #             n[maskx+i, masky+j] = 0
    #             n[i,j] = 1

    mask = mesh[0]**2 / a**2 + mesh[1]**2 / b**2 < 1.0
    values = (mesh[0]**2 / a**2 + mesh[1]**2 / b**2)

    values = (1 - np.exp(-0.05*values))**exp
    exp += dexp

    sigmoid = lspx
    sigmoid = 1-1/(1 + np.exp(-15*(sigmoid - 0.2)))
    sigmoid = np.tile(sigmoid.reshape(-1,1), (1,width))
    values *= sigmoid
    values /= values.max()
    r = np.zeros((height, width))
    r[mask] = values[mask]
    r /= r.max()

    # noise = np.zeros(n.shape)
    # noise[mask] = n[mask]
    # noise = scs.convolve2d(noise, gauss, mode='same')
    # noise = noise*sigmoid
    # r += noise
    # r /= r.max()

    # r[mesh[1] > 10] = 0
    # r = sigmoid
    r *= 255

    r = r.astype(np.uint8)
    new_img[h_offset:height+h_offset,c*width:(c+1)*width,0] = r
    new_img[h_offset:height+h_offset,c*width:(c+1)*width,1] = r
    new_img[h_offset:height+h_offset,c*width:(c+1)*width,2] = r
    new_img[h_offset:height+h_offset,c*width:(c+1)*width,3] = r
# new_img[:size, w*size:(w+1)*size, 0] = r
# new_img[:size, w*size:(w+1)*size, 1] = r
# new_img[:size, w*size:(w+1)*size, 2] = r
# new_img[:size, w*size:(w+1)*size, 3] = r

# import matplotlib.pyplot as plt
im = Img.fromarray(np.uint8(new_img), 'RGBA')

im.save("shield.png")
