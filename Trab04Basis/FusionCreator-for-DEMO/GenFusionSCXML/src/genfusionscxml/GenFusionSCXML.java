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
	fg.Redundancy(Speech.STOPSCROLL, SecondMod.STOPSCROLL, Output.STOPSCROLL);	
	fg.Redundancy(Speech.VERCARRINHO, SecondMod.VERCARRINHO, Output.VERCARRINHO);	
	
    fg.Complementary(SecondMod.scrollU, Speech.RAPIDO, Output.scrollUpRapido);
    fg.Complementary(SecondMod.scrollU, Speech.DEVAGAR, Output.scrollUpDevagar);
    fg.Complementary(SecondMod.scrollDR, Speech.DEVAGAR, Output.scrollDownDevagar);
    fg.Complementary(SecondMod.scrollDR, Speech.RAPIDO, Output.scrollDownRapido);
	
    //EXAMPLE
    
    fg.Build("fusion.scxml");
        
        
    }
    
}
