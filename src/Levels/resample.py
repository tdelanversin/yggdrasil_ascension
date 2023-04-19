import os
import shutil
import PIL.Image as Img
import numpy as np

# for obj in os.walk("."):
#     png_files = list(filter(lambda f: f.find(".png") >= 0, obj[2]))
#     png_files.sort()
#     if png_files:
#         for png_file in png_files:
#             p = png_file.split('-')
#             end = p[2].split('.')
#             new_name = p[0] + '-' + p[1] + '-' + str(int(end[0]) + offset) + "." + end[1]
#             os.rename(png_file, new_name)

file_name = "tileset_16.png"

img = np.array(Img.open(file_name))

new_img = np.zeros((2*img.shape[0], 2*img.shape[1], img.shape[2]), dtype = img.dtype)
# new_img[0:img.shape[0],0:img.shape[1],:] = img
new_img[0::2,0::2,:] = img
new_img[1::2,1::2,:] = img # 255*np.ones(img.shape)
new_img[0::2,1::2,:] = img
new_img[1::2,0::2,:] = img

# new_img = np.zeros(img.shape)

import matplotlib.pyplot as plt
im = Img.fromarray(np.uint8(new_img))

plt.imshow(new_img)
plt.show()

im.save("tileset_32.png")
