from typing import List


def choose(
  n: int, 
  k: int,
) -> int:
  """Iterative n choose k 

  Makes use of the fact that 
  choose(n, k+1) = choose(n, k) * ((n-k) / (k+1))

  """
  c = 1
  for i in range(k):
    c = c * (n - i) / (i + 1)
  return c


def chooseWithArray(
    n: int,
    k: int,
    bCoeffs: List[int],
) -> int:
    """n choose k given precomputed minimal array of binomial coefficients

    """
    # choose(n, k) == choose(n, n-k)
    if k > (n // 2): 
        k = n-k

    if (n % 2) == 0:
        i = (n // 2) * ((n//2) + 1)
    else:
        i = ((n+1)//2)*((n+1)//2)

    return bCoeffs[i+k]
        

    # TODO
    


def chooseSierpinskiNum(
    n: int,
    k: int,
    sierpinskiNum: int,
):
    """Return (choose(n, k) % 2) given Sierpinski Number.

    Sierpinski Number: Reduce minimal array for Pascal's Triangle mod 2
    and combine into an integer.
    See bCoeffsToSierpinskiNum

    """
    # TODO 
    pass



def bCoeffsAsArray(
  n_rows: int,
) -> List[int]:
  """Binomial coefficients in minimal array

  We can store unique values in Pascal's Triangle with M rows 
  in an array of size (M/2)*((M/2) + 1)

  ASSUMPTION: max_pascal_rows is even

  """
  # M Pascal rows needs (M/2)*((M/2)+1) entries
  pascal_storage: int = (n_rows // 2) * ((n_rows // 2) + 1)
  bCoeffs: List[int] = [1]*pascal_storage
  i = 0
  for n in range(n_rows):
    for k in range(0, (n//2 )+1):
      bCoeffs[i] = int(choose(n, k))
      i += 1
  return bCoeffs


def bCoeffsToSierpinskiNum(
    bCoeffs: List[int]
) -> int:
    """Convert binomnial coefficient array into Sierpinski Number

    Sierpinski gasket = (Pascal's triangle % 2) != 0
    If bCoeffs contains binomial coefficients for max_n = 2^k, 
    then we can take bCoeffs % 2 and combine into a single integer.
    
    ASSUMPTION: len(bCoeffs) is a power of 2

    """
    bCoeffsMod2 = map(
        lambda n : str(n % 2),
        bCoeffs,
    )
    sierpinskiStr = ''.join(bCoeffsMod2)
    return int(sierpinskiStr, 2)


def bCoeffsToDefString(
  bCoeffs: List[int]
) -> str:
  """Format bCoeffs array as a GLSL variable declaration

  """
  bCoeffStr: str = str(bCoeffs).replace('[', '').replace(']', '')
  nCoeffs: int = len(bCoeffs)
  return f'int[{nCoeffs}] BCOEFFS = int[{nCoeffs}]({bCoeffStr});'


# N Sierpinski iterations
N = 5 
# Sierpinski iteration N needs 2^N rows
MAX_PASCAL_ROWS = pow(2, N)

bCoeffs = bCoeffsAsArray(MAX_PASCAL_ROWS)
print(len(bCoeffs))
print(bCoeffsToSierpinskiNum(bCoeffs))
