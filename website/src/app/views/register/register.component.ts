import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { UserService } from '../../services/user.service';
import { Router, RouterLink } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'register',
  templateUrl: './register.component.html',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule, TranslateModule ,RouterLink],
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
  registerForm: FormGroup;
  isSubmitting = false;

  constructor(
    private fb: FormBuilder,
    private userService: UserService,
    private router: Router
  ) {
    this.registerForm = this.fb.group({
      username: ['', [Validators.required, Validators.minLength(3)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  onSubmit() {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;

    this.userService.register(this.registerForm.value).subscribe({
      next: (res: any) => {
        alert('Registrazione avvenuta con successo!');
        this.isSubmitting = false;
        this.registerForm.reset();
        this.router.navigate(['/']);
      },
      error: (err: any) => {
        this.isSubmitting = false;
        let serverMessage = err.error?.message || err.message || 'Errore interno del server.';

        if (err.status === 400) {
          alert('Errore: ' + serverMessage);
        } else if (err.status === 409) {
          alert('Errore: ' + serverMessage);
        } else {
          alert('Errore interno del server: ' + serverMessage);
        }
        console.error(err);
      }
    });
  }
}
