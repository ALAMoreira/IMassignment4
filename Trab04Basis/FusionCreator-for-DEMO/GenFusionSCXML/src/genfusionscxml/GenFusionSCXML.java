/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package genfusionscxml;

import java.io.IOException;
import scxmlgen.Fusion.FusionGenerator;
import scxmlgen.Modalities.Output;
import scxmlgen.Modalities.Speech;
import scxmlgen.Modalities.SecondMod;

/**
 *
 * @author nunof
 */
public class GenFusionSCXML {

    /**
     * @param args the command line arguments
     */
    public static void main(String[] args) throws IOException {

    FusionGenerator fg = new FusionGenerator();
	
	
	fg.Redundancy(Speech.AVANCAR, SecondMod.AvancarL, Output.AVANCAR);
	fg.Redundancy(Speech.RECUAR, SecondMod.RecuarR, Output.RECUAR);
	
	fg.Single(Speech.mcdonalds, Output.MCDONALDS);
	fg.Single(Speech.montaditos, Output.MONTADITOS);
	
    fg.Sequence(Speech.mcdonalds, Speech.UNIVERSIDADE, Output.MCDONALDS_UNIVERSIDADE);
	
    fg.Complementary(SecondMod.EsvaziarC, Speech.SIM, Output.ESVAZIARC);
    
	
    //EXAMPLE
    
    fg.Build("fusion.scxml");
        
        
    }
    
}
