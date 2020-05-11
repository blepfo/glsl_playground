import unittest

import sys
sys.path.append('../')
import pascal

class PascalTestCase(unittest.TestCase):
    
    def test_choose_iterative(self):
        choose = lambda n, k: pascal.choose(n, k)
        self.assertEqual(choose(0, 0), 1)
        self.assertEqual(choose(1, 0), 1)
        self.assertEqual(choose(3, 2), 3)
        self.assertEqual(choose(4, 1), 4)
        self.assertEqual(choose(4, 2), 6)
        self.assertEqual(choose(9, 4), 126)
        self.assertEqual(choose(10, 3), 120)
        self.assertEqual(choose(15, 3), 455)
        self.assertEqual(choose(15, 6), 5005)
        self.assertEqual(choose(15, 11), 1365)


    def test_bCoeffsAsArray(self):
        self.assertListEqual(
            pascal.bCoeffsAsArray(2), 
            [1, 1],
        )
        self.assertListEqual(
            pascal.bCoeffsAsArray(4), 
            [1, 1, 1, 2, 1, 3],
        )
        self.assertListEqual(
            pascal.bCoeffsAsArray(6), 
            [1, 1, 1, 2, 1, 3, 1, 4, 6, 1, 5, 10],
        )
        self.assertListEqual(
            pascal.bCoeffsAsArray(8), 
            [1, 1, 1, 2, 1, 3, 1, 4, 6, 1, 5, 10, 1, 6, 15, 20, 1, 7, 21, 35],
       ) 


    def test_chooseWithArray(self):
        bCoeffsArray4 = pascal.bCoeffsAsArray(6)
        choose = lambda n, k: pascal.chooseWithArray(n, k, bCoeffsArray4)
        self.assertEqual(choose(0, 0), 1)
        self.assertEqual(choose(1, 0), 1)
        self.assertEqual(choose(3, 2), 3)
        self.assertEqual(choose(4, 1), 4)
        self.assertEqual(choose(4, 2), 6)

        bCoeffsArray16 = pascal.bCoeffsAsArray(16)
        choose = lambda n, k: pascal.chooseWithArray(n, k, bCoeffsArray16)
        self.assertEqual(choose(0, 0), 1)
        self.assertEqual(choose(1, 0), 1)
        self.assertEqual(choose(3, 2), 3)
        self.assertEqual(choose(4, 1), 4)
        self.assertEqual(choose(4, 2), 6)
        self.assertEqual(choose(9, 4), 126)
        self.assertEqual(choose(10, 3), 120)
        self.assertEqual(choose(15, 3), 455)
        self.assertEqual(choose(15, 6), 5005)
        self.assertEqual(choose(15, 11), 1365)
        

if __name__ == "__main__":
    unittest.main()
