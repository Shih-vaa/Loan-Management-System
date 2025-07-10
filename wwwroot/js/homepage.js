document.addEventListener('DOMContentLoaded', function() {
    // Animate statistics counters
    const animateCounters = () => {
        const counters = document.querySelectorAll('.stat-number');
        const speed = 200; // Animation speed
        
        counters.forEach(counter => {
            const target = parseInt(counter.getAttribute('data-target') || 
                (counter.id === 'leads-processed' ? 1250 :
                 counter.id === 'active-users' ? 150 :
                 counter.id === 'loans-approved' ? 850 :
                 counter.id === 'teams-using' ? 25 : 0));
            
            const count = parseInt(counter.innerText);
            const increment = target / speed;
            
            if (count < target) {
                counter.innerText = Math.ceil(count + increment);
                setTimeout(animateCounters, 1);
            } else {
                counter.innerText = target.toLocaleString();
            }
        });
    };
    
    // Only animate when stats section is in view
    const statsSection = document.querySelector('.stats-section');
    const observer = new IntersectionObserver((entries) => {
        if (entries[0].isIntersecting) {
            animateCounters();
            observer.unobserve(statsSection);
        }
    }, { threshold: 0.5 });
    
    if (statsSection) {
        observer.observe(statsSection);
    }
    
    // Smooth scrolling for anchor links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function(e) {
            e.preventDefault();
            document.querySelector(this.getAttribute('href')).scrollIntoView({
                behavior: 'smooth'
            });
        });
    });
});