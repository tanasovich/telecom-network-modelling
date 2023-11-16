# Modelling of interference noises

Interference noise modelling using given data from configs and files. Produces **signal to noise** (SNR) results.
Performs calculation for *traditonal* systems.

## Input

To send input data, use file-based configuration approach. Check `appsettings.xml` file.

### Traditional systems' extra data

Application requires two files:

- impulse reactions
- spectral mask

Each file consists of list of numbers (one line). Single space is used as data separator

## Output

SNR values would be saved into text files. Each LT would have separate file with SNR array.
