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

size = 128
width = 15
new_img = np.zeros((size, width*size, 4), dtype = np.byte)
# new_img[0:img.shape[0],0:img.shape[1],:] = img

gauss = makeGaussian(13, 7)

import math
for w in range(0,width):
    r = np.random.random((size, size))
    r[r > 0.01] = 0
    r[r > 0] = 1

    lsp = np.linspace(-1, 1, size)
    mesh = np.meshgrid(lsp, lsp)
    r[np.sqrt(mesh[0]**2 + mesh[1]**2) > 0.75] = 0

    row = np.ones(17)
    masky = np.tile(row, (len(row),1))
    masky = ((masky * (np.ones(len(row))*np.arange(0,len(row)))).T - math.floor(len(row)/2)).astype(np.int32)
    maskx = masky.T
    for i in range(int(len(row)/2),size-int(len(row)/2)):
        for j in range(int(len(row)/2), size-int(len(row)/2)):
            if r[i,j] == 1:
                r[maskx+i, masky+j] = 0
                r[i,j] = 1

    r = scs.convolve2d(r, gauss, mode='same')
    # alpha = np.zeros((size, size), dtype=np.uint8)
    # alpha[r > 0] = 255
    # alpha = scs.convolve2d(alpha, makeGaussian(5, 2), mode='same')

    r *= 255
    new_img[:size, w*size:(w+1)*size, 0] = r
    new_img[:size, w*size:(w+1)*size, 1] = r
    new_img[:size, w*size:(w+1)*size, 2] = r
    new_img[:size, w*size:(w+1)*size, 3] = r

# import matplotlib.pyplot as plt
im = Img.fromarray(np.uint8(new_img), 'RGBA')

im.save("noise.png")
