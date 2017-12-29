# Miscellaneous information



## Lessons learned

* I have observed that there is no difference in timing if we compile a design in project mode or non-project mode in Vivado. This is important because it means that Vivado does not apply hidden settings while creating a project with the GUI.
* *Timing window diff from requirement* does not change if the same design is compiled using a different clock frequency. (A small difference is introduced by the precision of floating point operations.)
* While `Setup_fdre_C_D` is expected to be negative, Vivado sometimes says that it is positive [(more here)](https://forums.xilinx.com/t5/Timing-Analysis/I-was-fogged-by-the-data-required-time-in-Vivado/td-p/424596). Anyway, we use the end result (*Requirement plus delays*), so it does not screw up anything.
* If the critical path ends in a DPS48E1, *Timing window diff from requirement* will be much higher as the setup time of the DSP48E1 (~1.4ns) is higher than of the flipflops (0.06-0.183ns). Example for this: `mul_unsigned32_to_unsigned64_sync` (with -1.58ns of *Timing window diff from requirement*).


## Efforts on improving the approximation results

The following two ideas were considered to make the approximation results more accurate:
1. We could use the Transformer to create a testbed that included the operator of interest and all other Hastlayer infrastructure, then perform a timing analysis directly between the registers connected to the operator.
2. Even if we do not use the Transformer, we could run the implementation step on the design to get more realistic results.

The problems with (1.) are as follows:
* The STA only shows the critical paths by default. For certain single operations, the path corresponding to the operator is not the critical one, but we still need to know its timing parameters.
*That is difficult to do automatically because we need to know the names of the exact cells in the implemented design. Vivado generattes these names from the names of signals/variables.
* In addition, you simply can not find the cells for many signals/variables. This can be fixed by turning off optimizations for these signals (with KEEP and other similar attributes), however, this also modifies the topology of the implementation result, thus the paths and the data path delay as well.

Idea (2.) has actually been implemented. The issues with it are as follows:
* This is still not accurate, but at least shown higher delays than working only from synthesis.
* At the implementation stage the compiler checks if there are enough pins on the package of the FPGA chip for all inputs/outputs of the top level module. For the tests having 128 bit inputs and output, it fails for the current FPGA used on the Nexsys4DDR panel. This can certainly be fixed by only changing the UUT template. Shift registers could provide access to the bits of the inputs and the outputs. However, multiple system level changes had to be made to the Timing Tester to support that.
* For this reason, the implementation runs for some operators, and fails for some others. If the implementation succeeds, the timing parameters are calculated and printed into the log. In the corresponding branch, the `ImplementDesign` configuration switch can be used for turning that on.
