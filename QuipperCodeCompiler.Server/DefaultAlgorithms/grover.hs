import Quipper
import Quipper.Internal.Printing


cz :: (Qubit, Qubit) -> Circ (Qubit, Qubit)
cz (q1, q2) = do
    q2 <- hadamard q2
    q2 <- qnot q2 `controlled` q1
    q2 <- hadamard q2
    return (q1, q2)

oracle :: ( Qubit, Qubit ) -> Circ (Qubit , Qubit)
oracle (q1, q2) = do
    (q1, q2) <- cz(q1, q2)
    return (q1, q2)

grover :: ( Qubit , Qubit ) -> Circ ( Qubit , Qubit )
grover (q1 ,q2) = do
    (q1, q2) <- map_hadamard (q1,q2)

    -- Oracle - Quipper does not have a built in CZ gate, so we need to make one by doing: H(target) -> CNot -> H(target)
    (q1, q2) <- oracle (q1, q2)

    -- Diffusion operator
    --comment ""
    (q1, q2) <- map_hadamard (q1,q2)
    q1 <- gate_Z q1
    q2 <- gate_Z q2
    (q1, q2) <- cz(q1, q2)
    --comment ""
    (q1, q2) <- map_hadamard (q1,q2)
    return (q1, q2)


print_final :: IO ()
print_final =
    print_simple EPS $ do
        q1 <- qinit False
        q2 <- qinit False
        (q1', q2') <- grover (q1, q2)
        measure (q1', q2')


main :: IO ()
main = do
  print_final