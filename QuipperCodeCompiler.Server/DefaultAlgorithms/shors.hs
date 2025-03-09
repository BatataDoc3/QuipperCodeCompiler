import Quipper
import Control.Monad ( when, forM_, void )
import Data.List (replicate)

c_amod15_aux :: Int -> Int -> (Qubit, Qubit, Qubit, Qubit) -> Circ (Qubit, Qubit, Qubit, Qubit)
c_amod15_aux _ 0 c = return c
c_amod15_aux a power (q1, q2, q3, q4) = do
    when (a `elem` [2, 12]) $ do
        swap_at q3 q4
        swap_at q2 q3
        swap_at q1 q2
    when (a `elem` [7, 8]) $ do
        swap_at q1 q2
        swap_at q2 q3
        swap_at q3 q4
    when (a `elem` [4, 11]) $ do
        swap_at q2 q4
        swap_at q1 q3

    when (a `elem` [7, 11, 13]) $ do
        mapM_ gate_X [q1, q2, q3, q4]

    c_amod15_aux a (power - 1) (q1, q2, q3, q4)

c_amod15 :: Int -> Int -> (Qubit, Qubit, Qubit, Qubit) -> Circ (Qubit, Qubit, Qubit, Qubit)
c_amod15 a power (q1, q2, q3, q4) = do
    c_amod15_aux a power (q1, q2, q3, q4)



controlledPhase :: Qubit -> Qubit -> Double -> Circ ()
controlledPhase control target theta = do
  with_controls (control .==. 1) $ do
    global_phase theta

qftDagger :: Int -> [Qubit] -> Circ ()
qftDagger n qs = do


  let halfN = n `div` 2
  mapM_ (\i -> swap_at (qs !! i) (qs !! (n - i - 1))) [0 .. (halfN - 1)]

  mapM_ (\j -> do
    mapM_ (\m -> controlledPhase (qs !! m) (qs !! j) (-pi / (2^(j - m)))) [0 .. (j - 1)]
    hadamard_at (qs !! j)) [0 .. (n - 1)]

-- Function to box c_amod15
boxed_c_amod15 :: Int -> Int -> (Qubit, Qubit, Qubit, Qubit) -> Circ (Qubit, Qubit, Qubit, Qubit)
boxed_c_amod15 a power = box "c_amod15" (c_amod15 a power)

boxed_qftDagger :: Int -> [Qubit] -> Circ ()
boxed_qftDagger n = box "qftDagger" (qftDagger n)

shor's :: Circ [Bit]
shor's = do
    qubits <- qinit(replicate 12 False)
    mapM_ hadamard_at (take 8 qubits)
    gate_X (qubits !! 8)
    let a = 7
    mapM_ (\q -> when (q + 3 < 12) $
        void $ boxed_c_amod15 a (2^q) (qubits !! q, qubits !! (q+1), qubits !! (q+2), qubits !! (q+3))
        ) [0..7]
    boxed_qftDagger 8 (take 8 qubits)

    mapM measure (take 8 qubits)

print_final :: IO ()
print_final =  print_simple EPS (do
     shor's
    )

main :: IO ()
main = do
  print_final